Imports MySql.Data.MySqlClient
Imports System.Data

Public Class Form1
    Private applicants As New List(Of Applicant)()
    Private dbHelper As New DatabaseHelper()

    ' List of fixed positions for the ComboBox
    Private Positions As String() = {
        "IT",
        "Finance",
        "Engineering"
    }

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialize ComboBox with fixed positions
        InitializeComboBox()
        LoadJobsIntoComboBox()

        ' Test database connection
        If dbHelper.TestConnection() Then
            LoadApplicantsFromDatabase()
        End If

        InitializeForm()
    End Sub

    Private Sub InitializeComboBox()
        ' Clear any existing items
        ComboBox1.Items.Clear()

        ' Add fixed positions to ComboBox
        For Each position As String In Positions
            ComboBox1.Items.Add(position)
        Next

        ' Set default properties
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList ' Prevents typing
        ComboBox1.SelectedIndex = -1 ' No selection by default
    End Sub

    Public Sub LoadJobsIntoComboBox()
        Try
            ' Get jobs from database
            Dim jobsTable As DataTable = dbHelper.ExecuteQuery("SELECT Job_Name FROM jobs", Nothing)

            ' Add jobs to ComboBox if they don't already exist
            For Each row As DataRow In jobsTable.Rows
                Dim jobName As String = row("Job_Name").ToString()
                If Not ComboBox1.Items.Contains(jobName) Then
                    ComboBox1.Items.Add(jobName)
                End If
            Next
        Catch ex As Exception
            ' Silent fail - form still works with default positions
        End Try
    End Sub

    Private Sub InitializeForm()
        ' Set up default values
        TextBox1.Text = ""
        TextBox2.Text = ""
        ComboBox1.SelectedIndex = -1 ' No position selected
        DateTimePicker1.Value = DateTime.Now
    End Sub

    Private Sub LoadApplicantsFromDatabase()
        Try
            Dim query As String = "SELECT applicant_id, applicant_name, position, application_date, job_id FROM applicants ORDER BY applicant_id DESC"
            Dim dataTable As DataTable = dbHelper.ExecuteQuery(query, Nothing)

            applicants.Clear()
            For Each row As DataRow In dataTable.Rows
                Dim applicant As New Applicant(
                    row("applicant_id").ToString(),
                    row("applicant_name").ToString(),
                    row("position").ToString(),
                    Convert.ToDateTime(row("application_date"))
                )
                applicants.Add(applicant)
            Next

        Catch ex As Exception
            MessageBox.Show("Error loading applicants: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ValidateForm() Then
            If SaveApplicantToDatabase() Then
                MessageBox.Show("Applicant added successfully to database!", "XYZ Company", MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Reload applicants from database
                LoadApplicantsFromDatabase()

                ' Always open Form2 when Add is pressed
                OpenOrUpdateForm2()

                ClearForm()
            End If
        End If
    End Sub

    Private Function ValidateForm() As Boolean
        If String.IsNullOrWhiteSpace(TextBox1.Text) Then
            MessageBox.Show("Please enter Applicant ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            Return False
        End If

        Dim applicantId As Integer
        If Not Integer.TryParse(TextBox1.Text.Trim(), applicantId) Then
            MessageBox.Show("Applicant ID must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            TextBox1.SelectAll()
            Return False
        End If

        If String.IsNullOrWhiteSpace(TextBox2.Text) Then
            MessageBox.Show("Please enter applicant name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox2.Focus()
            Return False
        End If

        If ComboBox1.SelectedIndex = -1 OrElse ComboBox1.SelectedItem Is Nothing Then
            MessageBox.Show("Please select a position from the list.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            ComboBox1.Focus()
            Return False
        End If

        If DateTimePicker1.Value > DateTime.Now Then
            MessageBox.Show("Application date cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            DateTimePicker1.Focus()
            Return False
        End If

        ' Check if Applicant ID exists in employees table during validation
        If ApplicantIdExistsInEmployees(applicantId) Then
            MessageBox.Show("This Applicant ID is already registered as an employee and cannot be added as an applicant.", "Duplicate Employee ID", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            TextBox1.SelectAll()
            Return False
        End If

        If applicants.Any(Function(a) a.ApplicantID = TextBox1.Text.Trim()) Then
            MessageBox.Show("Applicant ID already exists in applicants. Please use a different ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            TextBox1.SelectAll()
            Return False
        End If

        Return True
    End Function

    Private Function SaveApplicantToDatabase() As Boolean
        Try
            ' Convert Applicant ID to integer
            Dim applicantId As Integer
            If Not Integer.TryParse(TextBox1.Text.Trim(), applicantId) Then
                MessageBox.Show("Applicant ID must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            ' Check if Applicant ID already exists in employees table
            If ApplicantIdExistsInEmployees(applicantId) Then
                MessageBox.Show("This Applicant ID is already registered as an employee and cannot be added as an applicant.", "Duplicate Employee ID", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TextBox1.Focus()
                TextBox1.SelectAll()
                Return False
            End If

            ' Get position as simple string
            Dim positionText As String = ComboBox1.SelectedItem.ToString()

            ' First create the job record with the same ID
            If Not CreateJobRecord(applicantId, positionText) Then
                MessageBox.Show("Failed to create job record. Please try again.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            ' Now job_id will be the same as applicant_id
            Dim jobId As Integer = applicantId

            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", applicantId},
                {"@name", TextBox2.Text.Trim()},
                {"@position", positionText},
                {"@appDate", DateTimePicker1.Value.Date},
                {"@job_id", jobId}  ' JOB_ID = APPLICANT_ID
            }

            ' MODIFIED QUERY TO INCLUDE JOB_ID
            Dim query As String = "INSERT INTO applicants (applicant_id, applicant_name, position, application_date, job_id) VALUES (@id, @name, @position, @appDate, @job_id)"
            Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(query, parameters)

            If rowsAffected > 0 Then
                Return True
            Else
                Return False
            End If

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show("Applicant ID already exists in applicants. Please use a different ID.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                MessageBox.Show("Error saving applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Catch ex As Exception
            MessageBox.Show("Error saving applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' Create job record with the same ID as applicant
    Private Function CreateJobRecord(jobId As Integer, position As String) As Boolean
        Try
            ' First check if job already exists
            Dim checkParams As New Dictionary(Of String, Object) From {
                {"@job_id", jobId}
            }
            Dim checkQuery As String = "SELECT COUNT(*) FROM jobs WHERE job_id = @job_id"
            Dim result As Object = dbHelper.ExecuteScalar(checkQuery, checkParams)

            If result IsNot Nothing AndAlso Convert.ToInt32(result) > 0 Then
                ' Job already exists, update it
                Dim updateParams As New Dictionary(Of String, Object) From {
                    {"@job_id", jobId},
                    {"@job_name", position},
                    {"@job_desc", $"{position} Department - Applicant {jobId}"}
                }
                Dim updateQuery As String = "UPDATE jobs SET Job_Name = @job_name, job_Description = @job_desc WHERE job_id = @job_id"
                dbHelper.ExecuteNonQuery(updateQuery, updateParams)
                Return True
            Else
                ' Create new job record
                Dim insertParams As New Dictionary(Of String, Object) From {
                    {"@job_id", jobId},
                    {"@job_name", position},
                    {"@job_desc", $"{position} Department - Applicant {jobId}"}
                }
                Dim insertQuery As String = "INSERT INTO jobs (job_id, Job_Name, job_Description) VALUES (@job_id, @job_name, @job_desc)"
                Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(insertQuery, insertParams)
                Return rowsAffected > 0
            End If

        Catch ex As Exception
            MessageBox.Show("Error creating job record: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' Check if Applicant ID exists in employees table
    Private Function ApplicantIdExistsInEmployees(applicantId As Integer) As Boolean
        Try
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", applicantId}
            }

            Dim query As String = "SELECT COUNT(*) FROM employees WHERE employee_id = @id"
            Dim result As Object = dbHelper.ExecuteScalar(query, parameters)

            Return result IsNot Nothing AndAlso Convert.ToInt32(result) > 0

        Catch ex As Exception
            MessageBox.Show("Error checking employee database: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ClearForm()
        MessageBox.Show("All fields have been cleared!", "XYZ Company", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to exit the application?",
                                                    "XYZ Company",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Application.Exit()
        End If
    End Sub

    Private Sub ClearForm()
        TextBox1.Clear()
        TextBox2.Clear()
        ComboBox1.SelectedIndex = -1 ' Clear ComboBox selection
        DateTimePicker1.Value = DateTime.Now
        TextBox1.Focus()
    End Sub

    Private Sub OpenOrUpdateForm2()
        Dim existingForm As Form2 = Nothing
        For Each form As Form In Application.OpenForms
            If TypeOf form Is Form2 Then
                existingForm = DirectCast(form, Form2)
                Exit For
            End If
        Next

        If existingForm IsNot Nothing Then
            existingForm.RefreshFromDatabase()
            existingForm.Show()
            existingForm.BringToFront()
        Else
            Dim form2 As New Form2(applicants)
            form2.Show()
        End If
    End Sub

    Public Function GetApplicants() As List(Of Applicant)
        Return applicants
    End Function

    Private Sub UpdateButtonStates() Handles TextBox1.TextChanged, TextBox2.TextChanged, ComboBox1.SelectedIndexChanged
        Dim hasData As Boolean = Not String.IsNullOrWhiteSpace(TextBox1.Text) AndAlso
                                Not String.IsNullOrWhiteSpace(TextBox2.Text) AndAlso
                                ComboBox1.SelectedIndex <> -1

        Button1.Enabled = hasData
    End Sub

End Class

Public Class Applicant
    Public Property ApplicantID As String
    Public Property ApplicantName As String
    Public Property Position As String
    Public Property ApplicationDate As DateTime

    Public Sub New()
    End Sub

    Public Sub New(id As String, name As String, pos As String, appDate As DateTime)
        ApplicantID = id
        ApplicantName = name
        Position = pos
        ApplicationDate = appDate
    End Sub
End Class