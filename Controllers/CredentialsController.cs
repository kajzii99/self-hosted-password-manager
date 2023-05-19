using System.Globalization;
using System.Security.Claims;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelfHostedPasswordManager.Data;
using SelfHostedPasswordManager.Encryption;
using SelfHostedPasswordManager.Models;
using SelfHostedPasswordManager.Services;

namespace SelfHostedPasswordManager.Controllers
{
    [Authorize]
    public class CredentialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EncryptionService _encryption;
        private readonly PasswordGeneratorService _passwordGenerator;

        public CredentialsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, EncryptionService encryption, PasswordGeneratorService passwordGenerator)
        {
            _context = context;
            _userManager = userManager;
            _encryption = encryption;
            _passwordGenerator = passwordGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentApplicationUserId = GetCurrentApplicationUserId();
            var userCredentials = await _context.Credentials.Where(cred => cred.ApplicationUserId.Equals(currentApplicationUserId)).ToListAsync();
            var hiddenUserCredentials = userCredentials.Select(cred => 
            {
                cred.Password = "###################";
                return cred; 
            }).ToList();

            return View(hiddenUserCredentials);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetPassword([Bind("Id")] string id)
        {
            var credential = await _context.Credentials.FirstOrDefaultAsync(m => m.Id == id);

            if (credential == null || credential.ApplicationUserId != GetCurrentApplicationUserId())
                return Content("-");

            return Content(_encryption.Decrypt(credential.Password));
        }

        [HttpGet]
        public IActionResult Export()
        {
            return View("_ExportCredentialsModalPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export([Bind("Password")] string password)
        {
            if(string.IsNullOrEmpty(password))
                return StatusCode(401);

            var currentApplicationUserId = GetCurrentApplicationUserId();
            var user = await _userManager.FindByIdAsync(currentApplicationUserId);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!isPasswordValid)
                return StatusCode(401);

            var userCredentials = await _context.Credentials.Where(cred => cred.ApplicationUserId.Equals(currentApplicationUserId)).ToListAsync();

            for(int i = 0; i < userCredentials.Count; i++)
            {
                userCredentials[i].ApplicationUserId = string.Empty;
                userCredentials[i].Password = _encryption.Decrypt(userCredentials[i].Password);
            }

            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ",",
                Encoding = Encoding.UTF8
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(userCredentials);
                }

                var content = stream.ToArray();

                return File(content, "text/csv", $"{User.Identity.Name.Split("@")[0]}-{DateTime.Now.ToShortDateString()}.csv");
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View("_ImportCredentialsModalPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile credentialsFile)
        {
            if (credentialsFile != null && credentialsFile.Length > 0)
            {
                if (!Path.GetExtension(credentialsFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("credentialsFile", "Nieprawidłowy typ pliku. Dozwolone tylko pliki .csv");
                    return View();
                }

                IEnumerable<Credential> credentials;

                using (var streamReader = new StreamReader(credentialsFile.OpenReadStream()))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                credentials = csvReader.GetRecords<Credential>().ToList();

                var currentApplicationUserId = GetCurrentApplicationUserId();
                var userCredentials = await _context.Credentials.Where(cred => cred.ApplicationUserId.Equals(currentApplicationUserId)).ToListAsync();

                foreach (Credential credential in credentials)
                {
                    if (userCredentials.Any(cred => cred.Id == credential.Id))
                        continue;

                    credential.ApplicationUserId = currentApplicationUserId;
                    credential.Password = _encryption.Encrypt(credential.Password);
                    _context.Credentials.Add(credential);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            Credential credential = new Credential();
            credential.Password = _passwordGenerator.GeneratePassword();

            return View("_CreateCredentialModalPartial", credential);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, Website, Username, Password, Notes")] Credential credential)
        {
            if (ModelState.IsValid)
            {
                credential.ApplicationUserId = GetCurrentApplicationUserId();
                credential.Password = _encryption.Encrypt(credential.Password);
                _context.Credentials.Add(credential);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(credential);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Credentials == null)
                return NotFound();

            var credential = await _context.Credentials.FindAsync(id);

            if (credential == null || credential.ApplicationUserId != GetCurrentApplicationUserId())
                return NotFound();

            credential.Password = _encryption.Decrypt(credential.Password);

            return View("_EditCredentialModalPartial", credential);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, Website, Username, Password, Notes")] Credential credential)
        {
            Credential cred = await _context.Credentials.FindAsync(credential.Id);

            if (cred == null)
            {
                // Zwracany 404 
                return StatusCode(404);
            }

            if (cred.ApplicationUserId != GetCurrentApplicationUserId())
                return StatusCode(401);

            if (ModelState.IsValid)
            {
                cred.Website = credential.Website;
                cred.Username = credential.Username;
                cred.Password = _encryption.Encrypt(credential.Password);
                cred.Notes = credential.Notes;

                try
                {
                    _context.Update(cred);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CredentialExists(credential.Id))
                        return StatusCode(403);

                    return StatusCode(400);
                }
                return StatusCode(200);
            }
            return StatusCode(400);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Credentials == null)
                return NotFound();

            var credential = await _context.Credentials.FirstOrDefaultAsync(m => m.Id == id);

            if (credential == null || credential.ApplicationUserId != GetCurrentApplicationUserId())
                return NotFound();

            return View("_DeleteCredentialModalPartial", credential);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Credentials == null)
                return Problem("Entity set 'ApplicationDbContext.credentials'  is null.");

            var credential = await _context.Credentials.FindAsync(id);
            
            if (credential != null && credential.ApplicationUserId == GetCurrentApplicationUserId())
                _context.Credentials.Remove(credential);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CredentialExists(string id)
        {
          return _context.Credentials.Any(e => e.Id == id);
        }

        private string GetCurrentApplicationUserId()
        {
            string applicationUserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            return applicationUserId;
        }
    }
}
