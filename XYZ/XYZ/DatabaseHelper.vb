Imports MySql.Data.MySqlClient
Imports System.Data

Public Class DatabaseHelper
    ' Connection string for MySQL Workbench
    Private connectionString As String = "Server=localhost;Database=XYZ;Uid=root;Pwd=1439;Port=3306;Allow User Variables=True;"

    Public Function GetConnection() As MySqlConnection
        Return New MySqlConnection(connectionString)
    End Function

    Public Function TestConnection() As Boolean
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Return True
            Catch ex As Exception
                MessageBox.Show("Database Connection Error: " & ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try
        End Using
    End Function

    Public Function ExecuteQuery(query As String, parameters As Dictionary(Of String, Object)) As DataTable
        Dim dataTable As New DataTable()

        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using command As New MySqlCommand(query, connection)
                    If parameters IsNot Nothing Then
                        For Each param In parameters
                            command.Parameters.AddWithValue(param.Key, param.Value)
                        Next
                    End If

                    Using adapter As New MySqlDataAdapter(command)
                        adapter.Fill(dataTable)
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Query Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using

        Return dataTable
    End Function

    Public Function ExecuteNonQuery(query As String, parameters As Dictionary(Of String, Object)) As Integer
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using command As New MySqlCommand(query, connection)
                    If parameters IsNot Nothing Then
                        For Each param In parameters
                            command.Parameters.AddWithValue(param.Key, param.Value)
                        Next
                    End If

                    Return command.ExecuteNonQuery()
                End Using
            Catch ex As Exception
                MessageBox.Show("Execution Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return -1
            End Try
        End Using
    End Function

    Public Function ExecuteScalar(query As String, parameters As Dictionary(Of String, Object)) As Object
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using command As New MySqlCommand(query, connection)
                    If parameters IsNot Nothing Then
                        For Each param In parameters
                            command.Parameters.AddWithValue(param.Key, param.Value)
                        Next
                    End If
                    Return command.ExecuteScalar()
                End Using
            Catch ex As Exception
                MessageBox.Show("Scalar Error: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return Nothing
            End Try
        End Using
    End Function
End Class