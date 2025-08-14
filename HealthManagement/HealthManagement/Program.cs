using System;
using System.Collections.Generic;
using System.Linq;

// Generic Repository for Entity Management
public class Repository<T>
{
    private List<T> items = new List<T>();

    public void Add(T item)
    {
        items.Add(item);
    }

    public List<T> GetAll()
    {
        return new List<T>(items); // Return a copy to maintain encapsulation
    }

    public T? GetById(Func<T, bool> predicate)
    {
        return items.FirstOrDefault(predicate);
    }

    public bool Remove(Func<T, bool> predicate)
    {
        var itemToRemove = items.FirstOrDefault(predicate);
        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
            return true;
        }
        return false;
    }
}

// Patient Entity
public class Patient
{
    public int Id { get; }
    public string Name { get; }
    public int Age { get; }
    public string Gender { get; }

    public Patient(int id, string name, int age, string gender)
    {
        Id = id;
        Name = name;
        Age = age;
        Gender = gender;
    }

    public override string ToString()
    {
        return $"Patient [ID: {Id}, Name: {Name}, Age: {Age}, Gender: {Gender}]";
    }
}

// Prescription Entity
public class Prescription
{
    public int Id { get; }
    public int PatientId { get; }
    public string MedicationName { get; }
    public DateTime DateIssued { get; }

    public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
    {
        Id = id;
        PatientId = patientId;
        MedicationName = medicationName;
        DateIssued = dateIssued;
    }

    public override string ToString()
    {
        return $"Prescription [ID: {Id}, PatientID: {PatientId}, Medication: {MedicationName}, Date: {DateIssued:yyyy-MM-dd}]";
    }
}

// Health System Application
public class HealthSystemApp
{
    public Repository<Patient> PatientRepo { get; private set; } = new Repository<Patient>();
    public Repository<Prescription> PrescriptionRepo { get; private set; } = new Repository<Prescription>();
    private Dictionary<int, List<Prescription>> prescriptionMap = new Dictionary<int, List<Prescription>>();

    public void SeedData()
    {
        Console.WriteLine("=== Seeding Sample Data ===");

        // Add Patients
        PatientRepo.Add(new Patient(1, "John Smith", 45, "Male"));
        PatientRepo.Add(new Patient(2, "Sarah Johnson", 32, "Female"));
        PatientRepo.Add(new Patient(3, "Michael Brown", 67, "Male"));

        Console.WriteLine("Added 3 patients to the system.");

        // Add Prescriptions with valid PatientIds
        PrescriptionRepo.Add(new Prescription(101, 1, "Lisinopril", DateTime.Now.AddDays(-10)));
        PrescriptionRepo.Add(new Prescription(102, 1, "Metformin", DateTime.Now.AddDays(-8)));
        PrescriptionRepo.Add(new Prescription(103, 2, "Ibuprofen", DateTime.Now.AddDays(-5)));
        PrescriptionRepo.Add(new Prescription(104, 3, "Atorvastatin", DateTime.Now.AddDays(-3)));
        PrescriptionRepo.Add(new Prescription(105, 2, "Amoxicillin", DateTime.Now.AddDays(-1)));

        Console.WriteLine("Added 5 prescriptions to the system.");
        Console.WriteLine();
    }

    public void BuildPrescriptionMap()
    {
        Console.WriteLine("=== Building Prescription Map ===");

        // Clear existing map
        prescriptionMap.Clear();

        // Group prescriptions by PatientId
        var allPrescriptions = PrescriptionRepo.GetAll();

        foreach (var prescription in allPrescriptions)
        {
            if (!prescriptionMap.ContainsKey(prescription.PatientId))
            {
                prescriptionMap[prescription.PatientId] = new List<Prescription>();
            }
            prescriptionMap[prescription.PatientId].Add(prescription);
        }

        Console.WriteLine($"Prescription map built successfully. Mapped {prescriptionMap.Count} patient(s).");
        Console.WriteLine();
    }

    public List<Prescription> GetPrescriptionsByPatientId(int patientId)
    {
        if (prescriptionMap.ContainsKey(patientId))
        {
            return new List<Prescription>(prescriptionMap[patientId]);
        }
        return new List<Prescription>();
    }

    public void PrintAllPatients()
    {
        Console.WriteLine("=== All Patients ===");
        var patients = PatientRepo.GetAll();

        if (patients.Count == 0)
        {
            Console.WriteLine("No patients found in the system.");
            return;
        }

        foreach (var patient in patients)
        {
            Console.WriteLine(patient.ToString());
        }
        Console.WriteLine();
    }

    public void PrintPrescriptionsForPatient(int patientId)
    {
        Console.WriteLine($"=== Prescriptions for Patient ID: {patientId} ===");

        // First, get patient details
        var patient = PatientRepo.GetById(p => p.Id == patientId);
        if (patient == null)
        {
            Console.WriteLine($"No patient found with ID: {patientId}");
            return;
        }

        Console.WriteLine($"Patient: {patient.Name}");
        Console.WriteLine();

        // Get prescriptions using the map
        var prescriptions = GetPrescriptionsByPatientId(patientId);

        if (prescriptions.Count == 0)
        {
            Console.WriteLine($"No prescriptions found for patient {patient.Name}.");
            return;
        }

        Console.WriteLine($"Found {prescriptions.Count} prescription(s):");
        foreach (var prescription in prescriptions.OrderByDescending(p => p.DateIssued))
        {
            Console.WriteLine($"  - {prescription.MedicationName} (Issued: {prescription.DateIssued:yyyy-MM-dd})");
        }
        Console.WriteLine();
    }

    public void PrintSystemSummary()
    {
        Console.WriteLine("=== System Summary ===");
        var totalPatients = PatientRepo.GetAll().Count;
        var totalPrescriptions = PrescriptionRepo.GetAll().Count;
        var patientsWithPrescriptions = prescriptionMap.Keys.Count;

        Console.WriteLine($"Total Patients: {totalPatients}");
        Console.WriteLine($"Total Prescriptions: {totalPrescriptions}");
        Console.WriteLine($"Patients with Prescriptions: {patientsWithPrescriptions}");

        if (totalPrescriptions > 0 && totalPatients > 0)
        {
            var avgPrescriptionsPerPatient = (double)totalPrescriptions / patientsWithPrescriptions;
            Console.WriteLine($"Average Prescriptions per Patient: {avgPrescriptionsPerPatient:F2}");
        }
        Console.WriteLine();
    }

    // Bonus method to demonstrate additional functionality
    public void FindPatientsByAge(int minAge, int maxAge)
    {
        Console.WriteLine($"=== Patients aged {minAge}-{maxAge} ===");
        var patients = PatientRepo.GetAll()
            .Where(p => p.Age >= minAge && p.Age <= maxAge)
            .OrderBy(p => p.Age)
            .ToList();

        if (patients.Count == 0)
        {
            Console.WriteLine($"No patients found in age range {minAge}-{maxAge}.");
            return;
        }

        foreach (var patient in patients)
        {
            var prescriptionCount = GetPrescriptionsByPatientId(patient.Id).Count;
            Console.WriteLine($"{patient.ToString()}, Prescriptions: {prescriptionCount}");
        }
        Console.WriteLine();
    }
}

// Program Entry Point
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Healthcare Management System ===");
        Console.WriteLine("Demonstrating Collections and Generics\n");

        // Instantiate the health system application
        var healthApp = new HealthSystemApp();

        // Execute the main workflow
        healthApp.SeedData();
        healthApp.BuildPrescriptionMap();
        healthApp.PrintAllPatients();

        // Display prescriptions for specific patients
        healthApp.PrintPrescriptionsForPatient(1); // John Smith
        healthApp.PrintPrescriptionsForPatient(2); // Sarah Johnson
        healthApp.PrintPrescriptionsForPatient(3); // Michael Brown

        // Additional demonstrations
        healthApp.PrintSystemSummary();
        healthApp.FindPatientsByAge(30, 50);

        // Demonstrate repository functionality
        Console.WriteLine("=== Testing Repository Functionality ===");

        // Find a specific patient
        var specificPatient = healthApp.PatientRepo.GetById(p => p.Name.Contains("Sarah"));
        if (specificPatient != null)
        {
            Console.WriteLine($"Found patient by name search: {specificPatient}");
        }

        // Test removal (commented out to keep data intact for demo)
        // bool removed = healthApp.PrescriptionRepo.Remove(p => p.Id == 105);
        // Console.WriteLine($"Prescription removal successful: {removed}");

        Console.WriteLine("\n=== Healthcare System Demo Complete ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}