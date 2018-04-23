Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports System.ComponentModel
Imports System.Data.SqlClient

Public Class People

	Private Shared _connectionString As String
	Private Shared _initialized As Boolean

	Public Shared Sub Initialize()
		' Initialize data source. Use "Northwind" connection string from configuration.

		If ConfigurationManager.ConnectionStrings("AdventureWorksConnectionString") Is Nothing OrElse ConfigurationManager.ConnectionStrings("AdventureWorksConnectionString").ConnectionString.Trim() = "" Then
			Throw New Exception("A connection string named 'AdventureWorksConnectionString' with a valid connection string " & "must exist in the <connectionStrings> configuration section for the application.")
		End If

		_connectionString = ConfigurationManager.ConnectionStrings("AdventureWorksConnectionString").ConnectionString

		_initialized = True
	End Sub

	<DataObjectMethod(DataObjectMethodType.Select, True)> _
	Public Shared Function GetPeople(ByVal startRecord As Int32, ByVal maxRecords As Int32, ByVal sortColumns As String) As DataTable
		VerifySortColumns(sortColumns)

		If (Not _initialized) Then
			Initialize()
		End If

		Dim sqlColumns As String = "[ContactID], [FirstName], [LastName], [EmailAddress], [Phone]"
		Dim sqlTable As String = "[Person].[Contact]"
		Dim sqlSortColumn As String = "[ContactID]"

		If (Not String.IsNullOrEmpty(sortColumns)) Then
			sqlSortColumn = sortColumns
		End If

		Dim sqlCommand As String = String.Format("SELECT * FROM (" & "    SELECT ROW_NUMBER() OVER (ORDER BY {0}) AS rownumber," & "        {1}" & "    FROM {2}" & ") AS foo " & "WHERE rownumber >= {3} AND rownumber <= {4}", sqlSortColumn, sqlColumns, sqlTable, startRecord + 1, startRecord + maxRecords)

		Dim conn As New SqlConnection(_connectionString)
		Dim da As New SqlDataAdapter(sqlCommand, conn)

		Dim ds As New DataSet()

		Try
			conn.Open()
			da.Fill(ds, "People")
		Catch e As SqlException
			' Handle exception.
		Finally
			conn.Close()
		End Try

		If ds.Tables("People") IsNot Nothing Then
			Return ds.Tables("People")
		End If

		Return Nothing
	End Function


	' columns are specified in the sort expression to avoid a SQL Injection attack.

	Private Shared Sub VerifySortColumns(ByVal sortColumns As String)
		sortColumns = sortColumns.ToLowerInvariant().Replace(" asc", String.Empty).Replace(" desc", String.Empty)
		Dim columnNames() As String = sortColumns.Split(","c)

		For Each columnName As String In columnNames

			Select Case columnName.Trim().ToLowerInvariant()
				Case "contactid", "firstname", "lastname", "emailaddress", "phone", ""
				Case Else
					Throw New ArgumentException("SortColumns contains an invalid column name.")
			End Select
		Next columnName
	End Sub

	Public Shared Function GetPeopleCount() As Int32
		If (Not _initialized) Then
			Initialize()
		End If

		Dim sqlCommand As String = "SELECT COUNT ([ContactID]) FROM [Person].[Contact]"

		Dim conn As New SqlConnection(_connectionString)
		Dim command As New SqlCommand(sqlCommand, conn)
		Dim da As New SqlDataAdapter(sqlCommand, conn)

		Dim result As Int32 = 0

		Try
			conn.Open()
			result = CInt(Fix(command.ExecuteScalar()))
		Catch e As SqlException
			' Handle exception.
		Finally
			conn.Close()
		End Try

		Return result
	End Function
End Class
