using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Intro_LINQ;

public partial class Form1 : Form
{
    private readonly List<Student> students = new();
    private readonly DataGridView dgvStudents = new();
    private readonly ComboBox cboProgram = new();
    private readonly NumericUpDown nudMinimumGpa = new();
    private readonly TextBox txtSearch = new();
    private readonly Label lblResults = new();

    public Form1()
    {
        Text = "Student Records Query Tool";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1120, 760);
        Font = new Font("Segoe UI", 9F);
        BuildInterface();
        LoadSampleStudents();
        DisplayStudents(students);
        SetResults("Ready. Choose a query or show all students.");
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(16), BackColor = Color.White };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var title = new Label { Text = "Student Records Query Tool", Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.FromArgb(35, 65, 100), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        root.Controls.Add(title, 0, 0);

        var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0, 5, 0, 5) };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.Controls.Add(filters, 0, 1);

        filters.Controls.Add(MakeLabel("Program:"), 0, 0);
        cboProgram.DropDownStyle = ComboBoxStyle.DropDownList;
        cboProgram.Dock = DockStyle.Fill;
        filters.Controls.Add(cboProgram, 1, 0);
        filters.Controls.Add(MakeLabel("Minimum GPA:"), 2, 0);
        nudMinimumGpa.DecimalPlaces = 2;
        nudMinimumGpa.Increment = 0.25M;
        nudMinimumGpa.Minimum = 0;
        nudMinimumGpa.Maximum = 4;
        nudMinimumGpa.Value = 3;
        nudMinimumGpa.Dock = DockStyle.Fill;
        filters.Controls.Add(nudMinimumGpa, 3, 0);
        filters.Controls.Add(MakeLabel("Search ID/name:"), 0, 1);
        txtSearch.Dock = DockStyle.Fill;
        txtSearch.PlaceholderText = "Enter student ID or part of a name";
        filters.Controls.Add(txtSearch, 1, 1);
        var tip = MakeLabel("Choose a button below to run a query.");
        tip.ForeColor = Color.DimGray;
        filters.Controls.Add(tip, 2, 1);
        filters.SetColumnSpan(tip, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(0, 8, 0, 0) };
        root.Controls.Add(actions, 0, 2);
        AddButton(actions, "Show All Students", (_, _) => ShowAllStudents());
        AddButton(actions, "Filter by Program", (_, _) => FilterByProgram());
        AddButton(actions, "Filter by GPA", (_, _) => FilterByGpa());
        AddButton(actions, "Search Student", (_, _) => SearchStudents());
        AddButton(actions, "Sort by Last Name", (_, _) => SortByName());
        AddButton(actions, "Sort by GPA", (_, _) => SortByGpa());
        AddButton(actions, "Combined Query", (_, _) => RunCombinedQuery());
        AddButton(actions, "Display Summary", (_, _) => DisplaySummary());

        var display = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        display.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        display.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.Controls.Add(display, 0, 3);

        dgvStudents.Dock = DockStyle.Fill;
        dgvStudents.ReadOnly = true;
        dgvStudents.AllowUserToAddRows = false;
        dgvStudents.AllowUserToDeleteRows = false;
        dgvStudents.AutoGenerateColumns = false;
        dgvStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvStudents.MultiSelect = false;
        dgvStudents.RowHeadersVisible = false;
        dgvStudents.BackgroundColor = Color.White;
        dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "Student ID", DataPropertyName = "StudentId", FillWeight = 75 });
        dgvStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFirst", HeaderText = "First Name", DataPropertyName = "FirstName" });
        dgvStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLast", HeaderText = "Last Name", DataPropertyName = "LastName" });
        dgvStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colProgram", HeaderText = "Program", DataPropertyName = "ProgramName", FillWeight = 150 });
        dgvStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colGpa", HeaderText = "GPA", DataPropertyName = "GPA", FillWeight = 65, DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" } });
        display.Controls.Add(dgvStudents, 0, 0);

        lblResults.Dock = DockStyle.Fill;
        lblResults.BorderStyle = BorderStyle.FixedSingle;
        lblResults.Padding = new Padding(8);
        lblResults.BackColor = Color.FromArgb(244, 247, 250);
        lblResults.TextAlign = ContentAlignment.MiddleLeft;
        lblResults.AutoEllipsis = true;
        display.Controls.Add(lblResults, 0, 1);
    }

    private static Label MakeLabel(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private static void AddButton(FlowLayoutPanel panel, string text, EventHandler action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 34, Padding = new Padding(8, 2, 8, 2), Margin = new Padding(0, 0, 8, 8), BackColor = Color.FromArgb(235, 241, 248), FlatStyle = FlatStyle.Flat };
        button.Click += action;
        panel.Controls.Add(button);
    }

    private void LoadSampleStudents()
    {
        students.AddRange(new[]
        {
            new Student { StudentId = 1001, FirstName = "Jordan", LastName = "Lee", ProgramName = "Software Development", GPA = 3.70 },
            new Student { StudentId = 1002, FirstName = "Morgan", LastName = "Reed", ProgramName = "Cybersecurity", GPA = 3.45 },
            new Student { StudentId = 1003, FirstName = "Taylor", LastName = "Brooks", ProgramName = "Data Analytics", GPA = 3.90 },
            new Student { StudentId = 1004, FirstName = "Casey", LastName = "Morgan", ProgramName = "Software Development", GPA = 2.85 },
            new Student { StudentId = 1005, FirstName = "Riley", LastName = "Patel", ProgramName = "Cybersecurity", GPA = 3.15 },
            new Student { StudentId = 1006, FirstName = "Avery", LastName = "Carter", ProgramName = "Data Analytics", GPA = 3.60 },
            new Student { StudentId = 1007, FirstName = "Jamie", LastName = "Nguyen", ProgramName = "Software Development", GPA = 3.25 },
            new Student { StudentId = 1008, FirstName = "Quinn", LastName = "Foster", ProgramName = "Cybersecurity", GPA = 2.70 },
            new Student { StudentId = 1009, FirstName = "Drew", LastName = "Bennett", ProgramName = "Data Analytics", GPA = 3.05 },
            new Student { StudentId = 1010, FirstName = "Parker", LastName = "Diaz", ProgramName = "Software Development", GPA = 4.00 },
            new Student { StudentId = 1011, FirstName = "Skyler", LastName = "Wright", ProgramName = "Cybersecurity", GPA = 3.80 },
            new Student { StudentId = 1012, FirstName = "Robin", LastName = "Adams", ProgramName = "Data Analytics", GPA = 2.95 }
        });
        cboProgram.Items.AddRange(students.Select(s => s.ProgramName).Distinct().OrderBy(p => p).Cast<object>().ToArray());
        if (cboProgram.Items.Count > 0) cboProgram.SelectedIndex = 0;
    }

    private void DisplayStudents(List<Student> records)
    {
        dgvStudents.DataSource = null;
        dgvStudents.DataSource = records;
        if (records.Count == 0) SetResults("No student records matched the query.");
        else SetResults($"Showing {records.Count} student record(s).");
    }

    private void ShowAllStudents() => DisplayStudents(students);

    private void FilterByProgram()
    {
        if (cboProgram.SelectedItem is not string selectedProgram) { SetResults("Select an academic program first."); return; }
        var results = students.Where(s => string.Equals(s.ProgramName, selectedProgram, StringComparison.OrdinalIgnoreCase)).ToList();
        DisplayStudents(results);
        SetResults(results.Count == 0 ? $"No students were found in {selectedProgram}." : $"{results.Count} student(s) found in {selectedProgram}.");
    }

    private void FilterByGpa()
    {
        double minimumGpa = (double)nudMinimumGpa.Value;
        if (minimumGpa < 0 || minimumGpa > 4) { SetResults("GPA must be between 0.00 and 4.00."); return; }
        var results = students.Where(s => s.GPA >= minimumGpa).ToList();
        DisplayStudents(results);
        SetResults(results.Count == 0 ? $"No students have a GPA of {minimumGpa:F2} or higher." : $"{results.Count} student(s) have a GPA of {minimumGpa:F2} or higher.");
    }

    private void SearchStudents()
    {
        string searchText = txtSearch.Text.Trim();
        if (string.IsNullOrWhiteSpace(searchText)) { SetResults("Enter a student ID or part of a first or last name."); return; }
        List<Student> results;
        if (int.TryParse(searchText, out int studentId))
        {
            Student? found = students.FirstOrDefault(s => s.StudentId == studentId);
            results = found is null ? new List<Student>() : new List<Student> { found };
        }
        else
        {
            results = students.Where(s => s.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || s.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        DisplayStudents(results);
        SetResults(results.Count == 0 ? "No student matched that ID or name." : $"{results.Count} matching student record(s) found.");
    }

    private void SortByName()
    {
        var results = students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();
        DisplayStudents(results);
        SetResults($"Showing {results.Count} student(s), sorted by last name and then first name.");
    }

    private void SortByGpa()
    {
        var results = students.OrderByDescending(s => s.GPA).ThenBy(s => s.LastName).ToList();
        DisplayStudents(results);
        Student? topStudent = students.OrderByDescending(s => s.GPA).FirstOrDefault();
        SetResults(topStudent is null ? "No student records are available." : $"Sorted highest to lowest GPA. Highest GPA: {topStudent.FirstName} {topStudent.LastName} ({topStudent.GPA:F2}).");
    }

    private void RunCombinedQuery()
    {
        if (cboProgram.SelectedItem is not string selectedProgram) { SetResults("Select an academic program before running the combined query."); return; }
        double minimumGpa = (double)nudMinimumGpa.Value;
        var results = students.Where(s => string.Equals(s.ProgramName, selectedProgram, StringComparison.OrdinalIgnoreCase) && s.GPA >= minimumGpa).OrderByDescending(s => s.GPA).ToList();
        DisplayStudents(results);
        SetResults(results.Count == 0 ? $"No {selectedProgram} students have a GPA of {minimumGpa:F2} or higher." : $"{results.Count} student(s) in {selectedProgram} have a GPA of at least {minimumGpa:F2}, sorted highest first.");
    }

    private void DisplaySummary()
    {
        if (students.Count == 0) { SetResults("No student records are available for summary calculations."); return; }
        int totalStudents = students.Count();
        double averageGpa = students.Average(s => s.GPA);
        double highestGpa = students.Max(s => s.GPA);
        double lowestGpa = students.Min(s => s.GPA);
        int studentsAtLeastThree = students.Count(s => s.GPA >= 3.0);
        SetResults($"SUMMARY | Total students: {totalStudents} | Average GPA: {averageGpa:F2} | Highest GPA: {highestGpa:F2} | Lowest GPA: {lowestGpa:F2} | GPA 3.00+: {studentsAtLeastThree}");
        DisplayStudents(students);
    }

    private void SetResults(string message) => lblResults.Text = message;
}
