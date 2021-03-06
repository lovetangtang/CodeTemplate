﻿Imports System

Namespace Northwind.DAL
	''' <summary>
	''' This object represents the properties and methods of a Supplier.
	''' </summary>
	Public Class Supplier
		Private _supplierID As Int32
		Private _companyName As String
		Private _contactName As String
		Private _contactTitle As String
		Private _address As String
		Private _city As String
		Private _region As String
		Private _postalCode As String
		Private _country As String
		Private _phone As String
		Private _fax As String
		Private _homePage As String

		Public Sub New()
		End Sub

		#region Custom - Methods
		' Insert custom methods in here so that they are preserved during re-generation.
#End Region
	End Class
End Namespace
