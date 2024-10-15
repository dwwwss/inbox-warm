using Microsoft.AspNetCore.Mvc;
using InboxWarmUp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using InboxWarmUp.ViewModels;
using System.Net.Mail; // For sending emails
using System.Net; // For email network settings
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace InboxWarmUp.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmailDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(EmailDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                    return View(model);
                }

                // Create a new user instance
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password, // Save password as plain text
                    CompanyName = model.CompanyName,
                    CompanySize = model.CompanySize,
                    CompanyWebsite = model.CompanyWebsite,
                    CompanyType = model.CompanyType
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login", "Account"); // Redirect after successful registration
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            // Implement your hashing logic here (e.g., using BCrypt or another hashing library)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
                if (user != null && user.Password == model.Password) // Ideally, compare hashed passwords
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    return RedirectToAction("Index", "Overview");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: /Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var model = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View("~/Views/Email/Profile.cshtml", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model, string newPassword)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.Id);
                if (user != null)
                {
                    // Update user properties
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    // Only update the password if provided and valid
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        var passwordHasher = new PasswordHasher<User>();
                        user.Password = passwordHasher.HashPassword(user, newPassword); // Hash the password
                    }

                    try
                    {
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                        return RedirectToAction("Profile"); // Redirect to the profile view after success
                    }
                    catch (DbUpdateException ex)
                    {
                        Console.WriteLine($"Database update failed: {ex.Message}");
                        ModelState.AddModelError("", "Failed to update the profile.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid data. Please check the input.");
            }

            // Return the same view with the model (and any validation errors)
            return View("~/Views/Email/Profile.cshtml", model);
        }



        // GET: /Account/Company
        [HttpGet]
        public IActionResult Company()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId.Value);
            if (user == null)
            {
                return NotFound("Company not found.");
            }

            var model = new User
            {
                CompanyName = user.CompanyName,
                CompanySize = user.CompanySize,
                CompanyWebsite = user.CompanyWebsite,
                CompanyType = user.CompanyType
            };

            return View("~/Views/Email/Company.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompany(User model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    return RedirectToAction("Login");
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.CompanyName = model.CompanyName;
                user.CompanySize = model.CompanySize;
                user.CompanyWebsite = model.CompanyWebsite;
                user.CompanyType = model.CompanyType;

                await _context.SaveChangesAsync();
                return RedirectToAction("Company");
            }

            return View("Company", model);
        }

        public IActionResult ForgotPassword()
        {
            ViewBag.PasswordSent = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    // Generate the new password
                    string newPassword = GenerateRandomPassword();

                    // Store the plain text password (not recommended for security reasons)
                    user.Password = newPassword; // Directly store the plain text password

                    try
                    {
                        // Save changes to the database
                        await _context.SaveChangesAsync();
                        // Set the standard subject for the email
                        string subject = "Your New Password";

                        // Prepare the email body using the plain text password
                        // Prepare the email body using the plain text password with a warm greeting
                        string emailBody = $@"
    <p>Dear User,</p>
    <p>We hope this message finds you well! Your new password is: <b>{newPassword}</b>.</p>
    <p>Please use this password to log in and remember to change it as soon as possible for your account's security.</p>
    <p>If you did not request a password reset, please ignore this email.</p>
    <p>Thank you for being a part of our community!</p>
    <p>Best regards,<br>InboxWarmUp</p>";


                        // Send the email with the same password
                        await _emailService.SendEmailAsync(
                            "rishika.dewang.averybit@gmail.com",
                            "xomb huip joap tkcw",
                            "Rishika",
                            model.Email,
                            subject,
                            emailBody,
                            "gmail"
                        );

                        ViewBag.PasswordSent = true; // Indicate that the password was sent successfully
                    }
                    catch (DbUpdateException dbEx)
                    {
                        // Log database update errors
                        Console.WriteLine($"Database update failed: {dbEx.Message}");
                        ViewBag.PasswordSent = false;
                        ModelState.AddModelError("", "Failed to update the password in the database.");
                    }
                    catch (Exception ex)
                    {
                        // Log other errors (like email sending issues)
                        Console.WriteLine($"Failed to send email: {ex}");
                        ViewBag.PasswordSent = false;
                        ModelState.AddModelError("", "Failed to send email. Please try again later.");
                    }
                }
                else
                {
                    ViewBag.PasswordSent = true; // Optional feedback for non-existing email
                }

                return View();
            }

            return View(model);
        }




        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            StringBuilder password = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[1];
                while (password.Length < length)
                {
                    rng.GetBytes(buffer);
                    char randomChar = (char)buffer[0];
                    if (validChars.Contains(randomChar))
                    {
                        password.Append(randomChar);
                    }
                }
            }
            return password.ToString();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return RedirectToAction("Profile");
            }

            // Check if the current password matches
            if (user.Password != CurrentPassword) // Compare plain text passwords
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return RedirectToAction("Profile");
            }

            // Update user password to new plain text password
            user.Password = NewPassword;

            // Ensure ModelState is valid
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Profile");
            }

            try
            {
                // Update user and save changes
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Password changed successfully!";
            }
            catch (DbUpdateException ex)
            {
                // Log detailed error information
                Console.WriteLine($"Database update failed: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                ModelState.AddModelError("", "Failed to change the password.");
            }

            return RedirectToAction("Profile");
        }



    }












}
