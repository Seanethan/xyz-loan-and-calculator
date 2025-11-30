Imports MySql.Data.MySqlClient
Imports System.Data

Public Class Form2
    Private applicantsList As List(Of Applicant)
    Private dbHelper As New DatabaseHelper()
    Private empId As Object

    Public Sub New(applicants As List(Of Applicant))
        InitializeComponent()
        applicantsList = applicants
        DisplayApplicants()
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "XYZ Company - Applicants Management"
        LoadApplicantsFromDatabase()
        SetupTableLayout()
    End Sub

    Private Sub LoadApplicantsFromDatabase()
        Try
            Dim query As String = "SELECT applicant_id, applicant_name, position, application_date, job_id FROM applicants ORDER BY applicant_id DESC"
            Dim dataTable As DataTable = dbHelper.ExecuteQuery(query, Nothing)

            If applicantsList Is Nothing Then
                applicantsList = New List(Of Applicant)()
            Else
                applicantsList.Clear()
            End If

            For Each row As DataRow In dataTable.Rows
                Dim applicantId As String = row("applicant_id").ToString()
                Dim applicantName As String = row("applicant_name").ToString()
                Dim position As String = row("position").ToString()
                Dim appDate As DateTime = Convert.ToDateTime(row("application_date"))

                Dim applicant As New Applicant(applicantId, applicantName, position, appDate)
                applicantsList.Add(applicant)
            Next

        Catch ex As Exception
            MessageBox.Show("Error loading applicants: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SetupTableLayout()
        TableLayoutPanel1.Controls.Clear()
        TableLayoutPanel1.ColumnCount = 6
        TableLayoutPanel1.ColumnStyles.Clear()
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))

        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Clear()
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))

        AddHeaderLabel("Applicant ID", 0, 0)
        AddHeaderLabel("Applicant Name", 1, 0)
        AddHeaderLabel("Position", 2, 0)
        AddHeaderLabel("Application Date", 3, 0)
        AddHeaderLabel("Hire", 4, 0)
        AddHeaderLabel("Reject", 5, 0)
    End Sub

    Private Sub AddHeaderLabel(text As String, column As Integer, row As Integer)
        Dim label As New Label()
        label.Text = text
        label.Font = New Font("Arial", 9, FontStyle.Bold)
        label.TextAlign = ContentAlignment.MiddleCenter
        label.BorderStyle = BorderStyle.FixedSingle
        label.BackColor = Color.LightBlue
        label.Dock = DockStyle.Fill
        TableLayoutPanel1.Controls.Add(label, column, row)
    End Sub

    Private Sub AddDataLabel(text As String, column As Integer, row As Integer)
        Dim label As New Label()
        label.Text = text
        label.Font = New Font("Arial", 9)
        label.TextAlign = ContentAlignment.MiddleLeft
        label.BorderStyle = BorderStyle.FixedSingle
        label.BackColor = Color.White
        label.Dock = DockStyle.Fill
        TableLayoutPanel1.Controls.Add(label, column, row)
    End Sub

    Private Sub AddHireButton(applicantId As String, applicantName As String, position As String, column As Integer, row As Integer)
        Dim btnHire As New Button()
        btnHire.Text = "Hire"
        btnHire.BackColor = Color.LightGreen
        btnHire.ForeColor = Color.Black
        btnHire.Font = New Font("Arial", 8, FontStyle.Bold)
        btnHire.Tag = New String() {applicantId, applicantName, position}
        btnHire.Dock = DockStyle.Fill
        AddHandler btnHire.Click, AddressOf HireButton_Click
        TableLayoutPanel1.Controls.Add(btnHire, column, row)
    End Sub

    Private Sub AddRejectButton(applicantId As String, column As Integer, row As Integer)
        Dim btnReject As New Button()
        btnReject.Text = "Reject"
        btnReject.BackColor = Color.LightCoral
        btnReject.ForeColor = Color.Black
        btnReject.Font = New Font("Arial", 8, FontStyle.Bold)
        btnReject.Tag = applicantId
        btnReject.Dock = DockStyle.Fill
        AddHandler btnReject.Click, AddressOf RejectButton_Click
        TableLayoutPanel1.Controls.Add(btnReject, column, row)
    End Sub

    Private Sub DisplayApplicants()
        SetupTableLayout()

        If applicantsList IsNot Nothing AndAlso applicantsList.Count > 0 Then
            TableLayoutPanel1.RowCount = applicantsList.Count + 1

            For i As Integer = 0 To applicantsList.Count - 1
                TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 35.0F))

                Dim applicant = applicantsList(i)
                AddDataLabel(applicant.ApplicantID, 0, i + 1)
                AddDataLabel(applicant.ApplicantName, 1, i + 1)
                AddDataLabel(applicant.Position, 2, i + 1)
                AddDataLabel(applicant.ApplicationDate.ToString("MM/dd/yyyy"), 3, i + 1)
                AddHireButton(applicant.ApplicantID, applicant.ApplicantName, applicant.Position, 4, i + 1)
                AddRejectButton(applicant.ApplicantID, 5, i + 1)
            Next

            Label2.Text = $"Total Applicants: {applicantsList.Count}"
        Else
            Label2.Text = "No applicants found in database"
            TableLayoutPanel1.RowCount = 2
            TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 25.0F))
            AddDataLabel("No applicants found in database", 0, 1)
            TableLayoutPanel1.SetColumnSpan(TableLayoutPanel1.GetControlFromPosition(0, 1), 6)
        End If
    End Sub

    ' ===== HIRE BUTTON FUNCTIONALITY =====
    Private Sub HireButton_Click(sender As Object, e As EventArgs)
        Dim btnHire As Button = DirectCast(sender, Button)
        Dim applicantData As String() = DirectCast(btnHire.Tag, String())

        Dim applicantId As String = applicantData(0)
        Dim applicantName As String = applicantData(1)
        Dim position As String = applicantData(2)

        Dim result As DialogResult = MessageBox.Show(
            $"Are you sure you want to hire {applicantName} as {position}?",
            "Confirm Hire",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        )

        If result = DialogResult.Yes Then
            If HireApplicant(applicantId, applicantName, position) Then
                MessageBox.Show($"{applicantName} has been hired successfully!", "Hire Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                RefreshFromDatabase()
            Else
                MessageBox.Show($"Failed to hire {applicantName}.", "Hire Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Private Function HireApplicant(applicantId As String, applicantName As String, position As String) As Boolean
        Try
            Dim empId As Integer
            If Not Integer.TryParse(applicantId, empId) Then
                MessageBox.Show("Invalid Applicant ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            ' First ensure job record exists for this employee
            If Not CreateOrUpdateJobRecord(empId, position, "Employee") Then
                MessageBox.Show("Failed to create job record for employee. Please try again.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            ' JOB_ID WILL BE THE SAME AS EMPLOYEE_ID
            Dim jobId As Integer = empId

            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", empId},
                {"@name", applicantName},
                {"@position", position},
                {"@hireDate", DateTime.Now.Date},
                {"@status", "Hired"},
                {"@job_id", jobId}  ' JOB_ID = EMPLOYEE_ID
            }

            Dim query As String = "INSERT INTO employees (employee_id, employee_name, position, hire_date, status, job_id) VALUES (@id, @name, @position, @hireDate, @status, @job_id)"
            Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(query, parameters)

            If rowsAffected > 0 Then
                Return True
            Else
                Return False
            End If

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show("This applicant is already hired!", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Else
                MessageBox.Show("Error hiring applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Catch ex As Exception
            MessageBox.Show("Error hiring applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' Create or update job record for employee
    Private Function CreateOrUpdateJobRecord(jobId As Integer, position As String, recordType As String) As Boolean
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
                    {"@job_desc", $"{position} Department - {recordType} {jobId}"}
                }
                Dim updateQuery As String = "UPDATE jobs SET Job_Name = @job_name, job_Description = @job_desc WHERE job_id = @job_id"
                dbHelper.ExecuteNonQuery(updateQuery, updateParams)
                Return True
            Else
                ' Create new job record
                Dim insertParams As New Dictionary(Of String, Object) From {
                    {"@job_id", jobId},
                    {"@job_name", position},
                    {"@job_desc", $"{position} Department - {recordType} {jobId}"}
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

    ' ===== REJECT BUTTON FUNCTIONALITY =====
    Private Sub RejectButton_Click(sender As Object, e As EventArgs)
        Dim btnReject As Button = DirectCast(sender, Button)
        Dim applicantId As String = DirectCast(btnReject.Tag, String)

        Dim applicant = applicantsList.FirstOrDefault(Function(a) a.ApplicantID = applicantId)
        Dim applicantName As String = If(applicant IsNot Nothing, applicant.ApplicantName, "this applicant")

        Dim result As DialogResult = MessageBox.Show(
            $"Are you sure you want to reject {applicantName}?",
            "Confirm Rejection",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If result = DialogResult.Yes Then
            If RejectApplicant(applicantId, applicantName) Then
                MessageBox.Show($"{applicantName} has been rejected!", "Rejection Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
                RefreshFromDatabase()
            Else
                MessageBox.Show($"Failed to reject {applicantName}.", "Rejection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub

    Private Function RejectApplicant(applicantId As String, applicantName As String) As Boolean
        Try
            Dim empId As Integer
            If Not Integer.TryParse(applicantId, empId) Then
                MessageBox.Show("Invalid Applicant ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            ' First ensure job record exists for rejected applicant
            If Not CreateOrUpdateJobRecord(empId, "Rejected", "Rejected Applicant") Then
                MessageBox.Show("Failed to create job record for rejected applicant. Please try again.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            ' JOB_ID WILL BE THE SAME AS EMPLOYEE_ID
            Dim jobId As Integer = empId

            Dim parameters As New Dictionary(Of String, Object) From {
                {"@id", empId},
                {"@name", applicantName},
                {"@position", "Rejected Applicant"},
                {"@hireDate", DateTime.Now.Date},
                {"@status", "Rejected"},
                {"@job_id", jobId}  ' JOB_ID = EMPLOYEE_ID
            }

            Dim query As String = "INSERT INTO employees (employee_id, employee_name, position, hire_date, status, job_id) VALUES (@id, @name, @position, @hireDate, @status, @job_id)"
            Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(query, parameters)

            If rowsAffected > 0 Then
                Return True
            Else
                Return False
            End If

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                ' Update status if already exists
                Dim updateParams As New Dictionary(Of String, Object) From {
                    {"@id", empId},
                    {"@status", "Rejected"}
                }
                Dim updateQuery As String = "UPDATE employees SET status = @status WHERE employee_id = @id"
                dbHelper.ExecuteNonQuery(updateQuery, updateParams)
                Return True
            Else
                MessageBox.Show("Error rejecting applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If
        Catch ex As Exception
            MessageBox.Show("Error rejecting applicant: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ' ===== BUTTONS FOR VIEWING STATUS =====
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ShowHiredApplicants()
    End Sub

    Private Sub ShowHiredApplicants()
        Try
            Dim query As String = "SELECT employee_id, employee_name, position, hire_date, status, job_id FROM employees WHERE status = 'Hired' ORDER BY employee_id DESC"
            Dim dataTable As DataTable = dbHelper.ExecuteQuery(query, Nothing)

            If dataTable.Rows.Count > 0 Then
                DisplayEmployeesInTable(dataTable, "Hired Employees")
            Else
                MessageBox.Show("No hired employees found.", "Hired Employees", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading hired employees: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ShowRejectedApplicants()
    End Sub

    Private Sub ShowRejectedApplicants()
        Try
            Dim query As String = "SELECT employee_id, employee_name, position, hire_date, status, job_id FROM employees WHERE status = 'Rejected' ORDER BY employee_id DESC"
            Dim dataTable As DataTable = dbHelper.ExecuteQuery(query, Nothing)

            If dataTable.Rows.Count > 0 Then
                DisplayEmployeesInTable(dataTable, "Rejected Applicants")
            Else
                MessageBox.Show("No rejected applicants found.", "Rejected Applicants", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading rejected applicants: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ShowHiringStatus()
    End Sub

    Private Sub ShowHiringStatus()
        Try
            Dim query As String = "SELECT employee_id, employee_name, position, hire_date, status, job_id FROM employees ORDER BY employee_id DESC"
            Dim dataTable As DataTable = dbHelper.ExecuteQuery(query, Nothing)

            If dataTable.Rows.Count > 0 Then
                DisplayEmployeesInTable(dataTable, "All Employees - Hiring Status")
            Else
                MessageBox.Show("No employee records found.", "Hiring Status", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading hiring status: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===== DISPLAY EMPLOYEES IN TABLE =====
    Private Sub DisplayEmployeesInTable(dataTable As DataTable, title As String)
        ' Create a new form to display the employees
        Dim statusForm As New Form()
        statusForm.Text = title
        statusForm.Size = New Size(800, 500)
        statusForm.StartPosition = FormStartPosition.CenterScreen

        ' Create TableLayoutPanel
        Dim tablePanel As New TableLayoutPanel()
        tablePanel.Dock = DockStyle.Fill
        tablePanel.ColumnCount = 6
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20.0F))
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20.0F))
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        tablePanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))

        tablePanel.RowCount = dataTable.Rows.Count + 1
        tablePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))

        ' Add headers
        AddEmployeeHeaderLabel("Employee ID", 0, 0, tablePanel)
        AddEmployeeHeaderLabel("Employee Name", 1, 0, tablePanel)
        AddEmployeeHeaderLabel("Position", 2, 0, tablePanel)
        AddEmployeeHeaderLabel("Hire Date", 3, 0, tablePanel)
        AddEmployeeHeaderLabel("Status", 4, 0, tablePanel)
        AddEmployeeHeaderLabel("Job ID", 5, 0, tablePanel)

        ' Add data rows
        For i As Integer = 0 To dataTable.Rows.Count - 1
            tablePanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))

            Dim row = dataTable.Rows(i)
            AddEmployeeDataLabel(row("employee_id").ToString(), 0, i + 1, tablePanel)
            AddEmployeeDataLabel(row("employee_name").ToString(), 1, i + 1, tablePanel)
            AddEmployeeDataLabel(row("position").ToString(), 2, i + 1, tablePanel)
            AddEmployeeDataLabel(Convert.ToDateTime(row("hire_date")).ToString("MM/dd/yyyy"), 3, i + 1, tablePanel)

            ' Color code the status
            Dim statusLabel As New Label()
            statusLabel.Text = row("status").ToString()
            statusLabel.Font = New Font("Arial", 9)
            statusLabel.TextAlign = ContentAlignment.MiddleCenter
            statusLabel.BorderStyle = BorderStyle.FixedSingle
            statusLabel.Dock = DockStyle.Fill

            If row("status").ToString() = "Hired" Then
                statusLabel.BackColor = Color.LightGreen
                statusLabel.ForeColor = Color.DarkGreen
            ElseIf row("status").ToString() = "Rejected" Then
                statusLabel.BackColor = Color.LightCoral
                statusLabel.ForeColor = Color.DarkRed
            Else
                statusLabel.BackColor = Color.White
            End If

            tablePanel.Controls.Add(statusLabel, 4, i + 1)

            ' Add Job ID column
            AddEmployeeDataLabel(row("job_id").ToString(), 5, i + 1, tablePanel)
        Next

        ' Add close button
        Dim closeButton As New Button()
        closeButton.Text = "Close"
        closeButton.Size = New Size(100, 30)
        closeButton.Location = New Point(350, 400)
        closeButton.Anchor = AnchorStyles.Bottom
        AddHandler closeButton.Click, Sub(s, ev) statusForm.Close()

        statusForm.Controls.Add(tablePanel)
        statusForm.Controls.Add(closeButton)

        statusForm.ShowDialog()
    End Sub

    Private Sub AddEmployeeHeaderLabel(text As String, column As Integer, row As Integer, tablePanel As TableLayoutPanel)
        Dim label As New Label()
        label.Text = text
        label.Font = New Font("Arial", 9, FontStyle.Bold)
        label.TextAlign = ContentAlignment.MiddleCenter
        label.BorderStyle = BorderStyle.FixedSingle
        label.BackColor = Color.LightBlue
        label.Dock = DockStyle.Fill
        tablePanel.Controls.Add(label, column, row)
    End Sub

    Private Sub AddEmployeeDataLabel(text As String, column As Integer, row As Integer, tablePanel As TableLayoutPanel)
        Dim label As New Label()
        label.Text = text
        label.Font = New Font("Arial", 9)
        label.TextAlign = ContentAlignment.MiddleLeft
        label.BorderStyle = BorderStyle.FixedSingle
        label.BackColor = Color.White
        label.Dock = DockStyle.Fill
        tablePanel.Controls.Add(label, column, row)
    End Sub

    ' ===== SEARCH FUNCTIONALITY =====
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SearchApplicant()
    End Sub

    Private Sub SearchApplicant()
        Dim searchTerm As String = TextBox1.Text.Trim()

        If String.IsNullOrEmpty(searchTerm) Then
            DisplayApplicants()
            Return
        End If

        LoadApplicantsFromDatabase()

        Dim searchTermLower = searchTerm.ToLower()
        Dim filteredApplicants = applicantsList.Where(Function(a)
                                                          Return (a.ApplicantID IsNot Nothing AndAlso a.ApplicantID.ToLower().Contains(searchTermLower)) OrElse
                                                                 (a.ApplicantName IsNot Nothing AndAlso a.ApplicantName.ToLower().Contains(searchTermLower)) OrElse
                                                                 (a.Position IsNot Nothing AndAlso a.Position.ToLower().Contains(searchTermLower))
                                                      End Function).ToList()

        DisplayFilteredApplicants(filteredApplicants, searchTerm)
    End Sub

    Private Sub DisplayFilteredApplicants(filteredList As List(Of Applicant), searchTerm As String)
        SetupTableLayout()

        If filteredList.Count > 0 Then
            TableLayoutPanel1.RowCount = filteredList.Count + 1

            For i As Integer = 0 To filteredList.Count - 1
                TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 35.0F))

                Dim applicant = filteredList(i)
                AddDataLabel(applicant.ApplicantID, 0, i + 1)
                AddDataLabel(applicant.ApplicantName, 1, i + 1)
                AddDataLabel(applicant.Position, 2, i + 1)
                AddDataLabel(applicant.ApplicationDate.ToString("MM/dd/yyyy"), 3, i + 1)

                AddHireButton(applicant.ApplicantID, applicant.ApplicantName, applicant.Position, 4, i + 1)
                AddRejectButton(applicant.ApplicantID, 5, i + 1)
            Next

            Label2.Text = $"Found {filteredList.Count} applicant(s) matching '{searchTerm}'"
        Else
            Label2.Text = $"No applicants found matching '{searchTerm}'"
            TableLayoutPanel1.RowCount = 2
            TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 25.0F))
            AddDataLabel($"No results found for '{searchTerm}'", 0, 1)
            TableLayoutPanel1.SetColumnSpan(TableLayoutPanel1.GetControlFromPosition(0, 1), 6)
        End If
    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            SearchApplicant()
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If String.IsNullOrEmpty(TextBox1.Text.Trim()) Then
            LoadApplicantsFromDatabase()
            DisplayApplicants()
        End If
    End Sub

    ' Method to refresh from database
    Public Sub RefreshFromDatabase()
        LoadApplicantsFromDatabase()
        DisplayApplicants()
    End Sub

    ' Refresh button functionality
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        RefreshFromDatabase()
        MessageBox.Show($"Data refreshed! Loaded {applicantsList.Count} applicants.", "XYZ Company", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class