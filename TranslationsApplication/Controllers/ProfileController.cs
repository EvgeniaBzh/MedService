using MedService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedService.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DbMedServiceContext _context;

        public ProfileController(DbMedServiceContext context)
        {
            _context = context;
        }

        [Authorize]
        public IActionResult Account()
        {
            var userEmail = User.Identity.Name;

            var doctor = _context.Doctors.SingleOrDefault(d => d.DoctorEmail == userEmail);
            if (doctor != null)
            {
                // Redirect to the Edit action of DoctorsController
                return RedirectToAction("Edit", "Doctors", new { id = doctor.DoctorId });
            }

            var patient = _context.Patients.SingleOrDefault(p => p.PatientEmail == userEmail);
            if (patient != null)
            {
                return View("PatientAccount", patient);
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateDoctor(Doctor doctor, IFormFile doctorPhoto)
        {
            if (!ModelState.IsValid)
            {
                return View("DoctorAccount", doctor);
            }

            var existingDoctor = await _context.Doctors.FindAsync(doctor.DoctorId);
            if (existingDoctor == null)
            {
                return NotFound();
            }

            existingDoctor.DoctorName = doctor.DoctorName;
            existingDoctor.DoctorPassword = doctor.DoctorPassword;

            if (doctorPhoto != null)
            {
                // Save the photo file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", doctorPhoto.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doctorPhoto.CopyToAsync(stream);
                }
                existingDoctor.DoctorPhoto = "/images/" + doctorPhoto.FileName;
            }

            existingDoctor.SpecializationId = doctor.SpecializationId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Account");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdatePatient(Patient patient, IFormFile medicalCardFile)
        {
            if (!ModelState.IsValid)
            {
                return View("PatientAccount", patient);
            }

            var existingPatient = await _context.Patients.FindAsync(patient.PatientId);
            if (existingPatient == null)
            {
                return NotFound();
            }

            existingPatient.PatientName = patient.PatientName;
            existingPatient.PatientPassword = patient.PatientPassword;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.PatientSex = patient.PatientSex;

            if (medicalCardFile != null)
            {
                // Save the medical card file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/medical-cards", medicalCardFile.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await medicalCardFile.CopyToAsync(stream);
                }
                existingPatient.MedicalCardFilePath = "/medical-cards/" + medicalCardFile.FileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Account");
        }

    }

}
