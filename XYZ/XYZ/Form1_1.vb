Imports MySql.Data.MySqlClient
Imports System.Data

Public Class Form1_1
    Private dbHelper As New DatabaseHelper()
    Private isSearching As Boolean = False
    Private lastSearchText As String = ""

    Private Sub Form1_1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set form title and focus
        Me.Text = "XYZ Company - Add Job"
        RadioButton1.Checked = True ' Set New as default
        SetNewJobMode()
        TextBox1.Focus()
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If RadioButton1.Checked Then
            SetNewJobMode()
        End If
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If RadioButton2.Checked Then
            SetExistingJobMode()
        End If
    End Sub

    Private Sub SetNewJobMode()
        ' Clear all fields
        TextBox1.Clear()
        TextBox2.Clear()
        TextBox3.Clear()

        ' Set Job ID as editable textbox
        TextBox1.ReadOnly = False
        TextBox1.BackColor = Color.White
        Label1.Text = "Job ID:"

        ' Enable Job Name and Description
        TextBox2.Enabled = True
        TextBox2.BackColor = Color.White
        TextBox3.Enabled = True
        TextBox3.BackColor = Color.White

        ' Change button text
        Button1.Text = "Add New Job"

        ' Set focus to Job ID
        TextBox1.Focus()
    End Sub

    Private Sub SetExistingJobMode()
        ' Clear fields but keep Job ID as search
        TextBox1.Clear()
        TextBox2.Clear()
        TextBox3.Clear()

        ' Set Job ID as search textbox
        TextBox1.ReadOnly = False
        TextBox1.BackColor = Color.LightYellow
        Label1.Text = "Search Job ID:"

        ' Disable Job Name and Description initially
        TextBox2.Enabled = False
        TextBox2.BackColor = Color.LightGray
        TextBox3.Enabled = False
        TextBox3.BackColor = Color.LightGray

        ' Change button text
        Button1.Text = "Update Job"

        ' Set focus to Job ID search
        TextBox1.Focus()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If RadioButton1.Checked Then
            ' New Job mode
            If ValidateJobForm() Then
                AddNewJob()
            End If
        Else
            ' Existing Job mode
            If ValidateExistingJobForm() Then
                UpdateExistingJob()
            End If
        End If
    End Sub

    Private Function ValidateJobForm() As Boolean
        ' Validate Job ID
        If String.IsNullOrWhiteSpace(TextBox1.Text) Then
            MessageBox.Show("Please enter Job ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            Return False
        End If

        If Not Integer.TryParse(TextBox1.Text.Trim(), Nothing) Then
            MessageBox.Show("Job ID must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            TextBox1.SelectAll()
            Return False
        End If

        ' Validate Job Name
        If String.IsNullOrWhiteSpace(TextBox2.Text) Then
            MessageBox.Show("Please enter Job Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox2.Focus()
            Return False
        End If

        ' Validate Job Description
        If String.IsNullOrWhiteSpace(TextBox3.Text) Then
            MessageBox.Show("Please enter Job Description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox3.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function ValidateExistingJobForm() As Boolean
        ' Validate Job ID
        If String.IsNullOrWhiteSpace(TextBox1.Text) Then
            MessageBox.Show("Please enter Job ID to search.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            Return False
        End If

        If Not Integer.TryParse(TextBox1.Text.Trim(), Nothing) Then
            MessageBox.Show("Job ID must be a number!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            TextBox1.SelectAll()
            Return False
        End If

        ' Only validate other fields if they're enabled (job was found)
        If TextBox2.Enabled Then
            If String.IsNullOrWhiteSpace(TextBox2.Text) Then
                MessageBox.Show("Please enter Job Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TextBox2.Focus()
                Return False
            End If

            If String.IsNullOrWhiteSpace(TextBox3.Text) Then
                MessageBox.Show("Please enter Job Description.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TextBox3.Focus()
                Return False
            End If
        Else
            MessageBox.Show("Please search for a valid Job ID first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            TextBox1.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub AddNewJob()
        Try
            Dim jobId As Integer = Integer.Parse(TextBox1.Text.Trim())
            Dim jobName As String = TextBox2.Text.Trim()
            Dim jobDescription As String = TextBox3.Text.Trim()

            ' Check if job ID already exists
            If JobIdExists(jobId) Then
                MessageBox.Show("Job ID already exists! Please use a different ID.", "Duplicate Job ID", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TextBox1.Focus()
                TextBox1.SelectAll()
                Return
            End If

            ' Insert the new job into database
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@job_id", jobId},
                {"@Job_Name", jobName},
                {"@job_Description", jobDescription}
            }

            Dim query As String = "INSERT INTO jobs (job_id, Job_Name, job_Description) VALUES (@job_id, @Job_Name, @job_Description)"
            Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(query, parameters)

            If rowsAffected > 0 Then
                MessageBox.Show($"Job '{jobName}' added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                NavigateToForm1()
            Else
                MessageBox.Show("Failed to add job to database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                MessageBox.Show("Job ID already exists in the database!", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Else
                MessageBox.Show("Database Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("Error adding job: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateExistingJob()
        Try
            Dim jobId As Integer = Integer.Parse(TextBox1.Text.Trim())
            Dim jobName As String = TextBox2.Text.Trim()
            Dim jobDescription As String = TextBox3.Text.Trim()

            ' Check if job ID exists
            If Not JobIdExists(jobId) Then
                MessageBox.Show("Job ID does not exist! Please enter a valid Job ID.", "Job Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                TextBox1.Focus()
                TextBox1.SelectAll()
                Return
            End If

            ' Update the existing job in database
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@job_id", jobId},
                {"@Job_Name", jobName},
                {"@job_Description", jobDescription}
            }

            Dim query As String = "UPDATE jobs SET Job_Name = @Job_Name, job_Description = @job_Description WHERE job_id = @job_id"
            Dim rowsAffected As Integer = dbHelper.ExecuteNonQuery(query, parameters)

            If rowsAffected > 0 Then
                MessageBox.Show($"Job '{jobName}' updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                NavigateToForm1()
            Else
                MessageBox.Show("Failed to update job in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        Catch ex As Exception
            MessageBox.Show("Error updating job: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If RadioButton2.Checked AndAlso Not String.IsNullOrWhiteSpace(TextBox1.Text) Then
            ' Use a timer to delay search to avoid rapid database queries
            If TextBox1.Text <> lastSearchText Then
                lastSearchText = TextBox1.Text
                Timer1.Stop()
                Timer1.Start()
            End If
        End If
        UpdateButtonState()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        If RadioButton2.Checked Then
            SearchJobById()
        End If
    End Sub

    Private Sub SearchJobById()
        If String.IsNullOrWhiteSpace(TextBox1.Text) OrElse isSearching Then
            Return
        End If

        If Not Integer.TryParse(TextBox1.Text.Trim(), Nothing) Then
            ' Clear fields if not a valid number
            ClearJobFields()
            Return
        End If

        Try
            isSearching = True
            Dim jobId As Integer = Integer.Parse(TextBox1.Text.Trim())
            Dim jobData As DataTable = GetJobById(jobId)

            If jobData.Rows.Count > 0 Then
                ' Job found, populate fields
                Dim row As DataRow = jobData.Rows(0)
                TextBox2.Text = row("Job_Name").ToString()
                TextBox3.Text = row("job_Description").ToString()

                ' Enable the fields for editing
                TextBox2.Enabled = True
                TextBox2.BackColor = Color.White
                TextBox3.Enabled = True
                TextBox3.BackColor = Color.White
            Else
                ' Job not found, clear and disable fields
                ClearJobFields()
            End If

        Catch ex As Exception
            ' Silent fail - don't show error for search
            ClearJobFields()
        Finally
            isSearching = False
        End Try
    End Sub

    Private Sub ClearJobFields()
        TextBox2.Clear()
        TextBox3.Clear()
        TextBox2.Enabled = False
        TextBox2.BackColor = Color.LightGray
        TextBox3.Enabled = False
        TextBox3.BackColor = Color.LightGray
    End Sub

    Private Function GetJobById(jobId As Integer) As DataTable
        Try
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@job_id", jobId}
            }
            Dim query As String = "SELECT job_id, Job_Name, job_Description FROM jobs WHERE job_id = @job_id"
            Return dbHelper.ExecuteQuery(query, parameters)
        Catch ex As Exception
            Return New DataTable()
        End Try
    End Function

    Private Sub NavigateToForm1()
        Me.Hide()

        Dim existingForm1 As Form1 = Nothing
        For Each form As Form In Application.OpenForms
            If TypeOf form Is Form1 Then
                existingForm1 = DirectCast(form, Form1)
                Exit For
            End If
        Next

        If existingForm1 IsNot Nothing Then
            existingForm1.Show()
            existingForm1.BringToFront()
            existingForm1.LoadJobsIntoComboBox()
        Else
            Dim form1 As New Form1()
            form1.Show()
        End If
    End Sub

    Private Function JobIdExists(jobId As Integer) As Boolean
        Try
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@job_id", jobId}
            }
            Dim query As String = "SELECT COUNT(*) FROM jobs WHERE job_id = @job_id"
            Dim result As Object = dbHelper.ExecuteScalar(query, parameters)
            Return result IsNot Nothing AndAlso Convert.ToInt32(result) > 0
        Catch ex As Exception
            Return False
        End Try
    End Function

    ' Keyboard shortcuts
    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            If RadioButton1.Checked Then
                TextBox2.Focus()
            Else
                ' In existing mode, pressing Enter triggers immediate search
                Timer1.Stop()
                SearchJobById()
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox2.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            TextBox3.Focus()
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox3_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox3.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            Button1.PerformClick()
            e.Handled = True
        End If
    End Sub

    ' Text changed events to enable/disable button
    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        UpdateButtonState()
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        UpdateButtonState()
    End Sub

    Private Sub UpdateButtonState()
        If RadioButton1.Checked Then
            ' New Job mode - enable if all fields have data
            Button1.Enabled = Not String.IsNullOrWhiteSpace(TextBox1.Text) AndAlso
                             Not String.IsNullOrWhiteSpace(TextBox2.Text) AndAlso
                             Not String.IsNullOrWhiteSpace(TextBox3.Text)
        Else
            ' Existing Job mode - enable if Job ID is entered and fields are populated and enabled
            Button1.Enabled = Not String.IsNullOrWhiteSpace(TextBox1.Text) AndAlso
                             Not String.IsNullOrWhiteSpace(TextBox2.Text) AndAlso
                             Not String.IsNullOrWhiteSpace(TextBox3.Text) AndAlso
                             TextBox2.Enabled
        End If
    End Sub
End Class