using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Student Class
public class Student
{
    public int Id { get; }
    public string FullName { get; }
    public int Score { get; }

    public Student(int id, string fullName, int score)
    {
        Id = id;
        FullName = fullName;
        Score = score;
    }

    public string GetGrade()
    {
        if (Score >= 80 && Score <= 100)
            return "A";
        else if (Score >= 70 && Score <= 79)
            return "B";
        else if (Score >= 60 && Score <= 69)
            return "C";
        else if (Score >= 50 && Score <= 59)
            return "D";
        else
            return "F";
    }

    public override string ToString()
    {
        return $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
    }
}

// Custom Exception Classes
public class InvalidScoreFormatException : Exception
{
    public InvalidScoreFormatException(string message) : base(message) { }

    public InvalidScoreFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class MissingFieldException : Exception
{
    public MissingFieldException(string message) : base(message) { }
}

// Student Result Processor Class
public class StudentResultProcessor
{
    public List<Student> ReadStudentsFromFile(string inputFilePath)
    {
        var students = new List<Student>();
        int lineNumber = 0;

        using (var reader = new StreamReader(inputFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine($"Warning: Skipping empty line {lineNumber}");
                    continue;
                }

                try
                {
                    // Split the line by comma and trim whitespace
                    string[] fields = line.Split(',')
                                          .Select(field => field.Trim())
                                          .ToArray();

                    // Validate number of fields
                    if (fields.Length < 3)
                    {
                        throw new MissingFieldException(
                            $"Line {lineNumber}: Expected 3 fields (ID, FullName, Score) but found {fields.Length}. " +
                            $"Line content: '{line}'");
                    }

                    if (fields.Length > 3)
                    {
                        Console.WriteLine($"Warning: Line {lineNumber} has {fields.Length} fields. Using first 3 fields only.");
                    }

                    // Extract and validate fields
                    string idField = fields[0];
                    string fullName = fields[1];
                    string scoreField = fields[2];

                    // Validate that required fields are not empty
                    if (string.IsNullOrWhiteSpace(idField) ||
                        string.IsNullOrWhiteSpace(fullName) ||
                        string.IsNullOrWhiteSpace(scoreField))
                    {
                        throw new MissingFieldException(
                            $"Line {lineNumber}: One or more fields are empty. " +
                            $"ID: '{idField}', Name: '{fullName}', Score: '{scoreField}'");
                    }

                    // Try to parse ID
                    int id;
                    if (!int.TryParse(idField, out id))
                    {
                        throw new InvalidScoreFormatException(
                            $"Line {lineNumber}: Student ID '{idField}' is not a valid integer.");
                    }

                    // Try to parse score
                    int score;
                    if (!int.TryParse(scoreField, out score))
                    {
                        throw new InvalidScoreFormatException(
                            $"Line {lineNumber}: Score '{scoreField}' is not a valid integer.");
                    }

                    // Validate score range (optional - for data quality)
                    if (score < 0 || score > 100)
                    {
                        Console.WriteLine($"Warning: Line {lineNumber} - Score {score} is outside typical range (0-100)");
                    }

                    // Create and add student
                    var student = new Student(id, fullName, score);
                    students.Add(student);

                    Console.WriteLine($"✓ Successfully processed: {student.FullName} (Line {lineNumber})");
                }
                catch (MissingFieldException)
                {
                    // Re-throw custom exceptions to be handled by caller
                    throw;
                }
                catch (InvalidScoreFormatException)
                {
                    // Re-throw custom exceptions to be handled by caller
                    throw;
                }
                catch (Exception ex)
                {
                    // Wrap unexpected exceptions with context
                    throw new Exception($"Unexpected error processing line {lineNumber}: '{line}'", ex);
                }
            }
        }

        Console.WriteLine($"\nTotal students successfully read: {students.Count}");
        return students;
    }

    public void WriteReportToFile(List<Student> students, string outputFilePath)
    {
        using (var writer = new StreamWriter(outputFilePath))
        {
            // Write header
            writer.WriteLine("=== SCHOOL GRADING REPORT ===");
            writer.WriteLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine($"Total Students: {students.Count}");
            writer.WriteLine();

            // Write individual student results
            writer.WriteLine("STUDENT RESULTS:");
            writer.WriteLine(new string('-', 60));

            foreach (var student in students.OrderBy(s => s.Id))
            {
                writer.WriteLine(student.ToString());
            }

            // Write grade distribution summary
            writer.WriteLine();
            writer.WriteLine("GRADE DISTRIBUTION:");
            writer.WriteLine(new string('-', 30));

            var gradeDistribution = students.GroupBy(s => s.GetGrade())
                                          .OrderBy(g => g.Key)
                                          .ToDictionary(g => g.Key, g => g.Count());

            foreach (var grade in new[] { "A", "B", "C", "D", "F" })
            {
                int count = gradeDistribution.ContainsKey(grade) ? gradeDistribution[grade] : 0;
                double percentage = students.Count > 0 ? (count * 100.0 / students.Count) : 0;
                writer.WriteLine($"Grade {grade}: {count} students ({percentage:F1}%)");
            }

            // Write statistics
            if (students.Count > 0)
            {
                writer.WriteLine();
                writer.WriteLine("STATISTICS:");
                writer.WriteLine(new string('-', 20));
                writer.WriteLine($"Highest Score: {students.Max(s => s.Score)}");
                writer.WriteLine($"Lowest Score: {students.Min(s => s.Score)}");
                writer.WriteLine($"Average Score: {students.Average(s => s.Score):F2}");
            }
        }

        Console.WriteLine($"✓ Report successfully written to: {outputFilePath}");
    }

    // Helper method to create sample input file for testing
    public void CreateSampleInputFile(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("101, Alice Smith, 84");
            writer.WriteLine("102, Bob Johnson, 72");
            writer.WriteLine("103, Carol Davis, 95");
            writer.WriteLine("104, David Brown, 67");
            writer.WriteLine("105, Emma Wilson, 45");
            writer.WriteLine("106, Frank Miller, 88");
            writer.WriteLine("107, Grace Taylor, 76");
            writer.WriteLine("108, Henry Anderson, 52");
            writer.WriteLine("109, Ivy Thomas, 91");
            writer.WriteLine("110, Jack White, 63");
        }
        Console.WriteLine($"✓ Sample input file created: {filePath}");
    }

    // Helper method to demonstrate various error scenarios
    public void CreateErrorTestFile(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("201, Valid Student, 85");
            writer.WriteLine("202, Missing Score"); // Missing field
            writer.WriteLine("203, Invalid Score, ABC"); // Invalid score format
            writer.WriteLine(", Empty ID, 75"); // Empty ID
            writer.WriteLine("204, , 80"); // Empty name
            writer.WriteLine("205, Another Valid, 92");
            writer.WriteLine(""); // Empty line
            writer.WriteLine("206, Extra Field Student, 78, Extra, Data"); // Extra fields
        }
        Console.WriteLine($"✓ Error test file created: {filePath}");
    }
}

// Program Entry Point
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== School Grading System ===");
        Console.WriteLine("Demonstrating File I/O and Exception Handling\n");

        var processor = new StudentResultProcessor();

        // File paths
        string inputFile = "students_input.txt";
        string outputFile = "grade_report.txt";
        string errorTestFile = "students_errors.txt";

        try
        {
            // Create sample files for demonstration
            Console.WriteLine("=== Creating Sample Files ===");
            processor.CreateSampleInputFile(inputFile);
            processor.CreateErrorTestFile(errorTestFile);
            Console.WriteLine();

            // Process the main input file
            Console.WriteLine("=== Processing Main Input File ===");
            var students = processor.ReadStudentsFromFile(inputFile);
            processor.WriteReportToFile(students, outputFile);
            Console.WriteLine();

            // Demonstrate error handling with problematic file
            Console.WriteLine("=== Testing Error Handling ===");
            try
            {
                var errorStudents = processor.ReadStudentsFromFile(errorTestFile);
                processor.WriteReportToFile(errorStudents, "error_report.txt");
            }
            catch (MissingFieldException ex)
            {
                Console.WriteLine($"✗ Missing Field Error: {ex.Message}");
            }
            catch (InvalidScoreFormatException ex)
            {
                Console.WriteLine($"✗ Invalid Score Format Error: {ex.Message}");
            }

            // Test file not found scenario
            Console.WriteLine("\n=== Testing File Not Found ===");
            try
            {
                processor.ReadStudentsFromFile("nonexistent_file.txt");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"✗ File Not Found: {ex.Message}");
            }

        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"✗ File Not Found Error: {ex.Message}");
            Console.WriteLine("Please ensure the input file exists and try again.");
        }
        catch (InvalidScoreFormatException ex)
        {
            Console.WriteLine($"✗ Invalid Score Format: {ex.Message}");
            Console.WriteLine("Please check that all scores are valid integers.");
        }
        catch (MissingFieldException ex)
        {
            Console.WriteLine($"✗ Missing Field Error: {ex.Message}");
            Console.WriteLine("Please ensure each line has ID, FullName, and Score separated by commas.");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"✗ Access Denied: {ex.Message}");
            Console.WriteLine("Please check file permissions and try again.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"✗ File I/O Error: {ex.Message}");
            Console.WriteLine("There was a problem reading or writing the file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Unexpected Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }

        Console.WriteLine("\n=== Grading System Demo Complete ===");
        Console.WriteLine("Check the output files for the generated reports.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}