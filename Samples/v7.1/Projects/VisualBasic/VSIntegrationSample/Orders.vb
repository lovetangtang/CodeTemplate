Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Data
Imports System.Data.SqlClient

Namespace VSIntegrationSample
#Region "Order"
''' <summary>
''' This object represents the properties and methods of a Orders.
''' </summary>
Public Class Order
	Private _id As System.Int32
	Private Dim _userId As System.String = String.Empty
	Private Dim _orderDate As System.DateTime
	Private Dim _shipAddr1 As System.String = String.Empty
	Private Dim _shipAddr2 As System.String = String.Empty
	Private Dim _shipCity As System.String = String.Empty
	Private Dim _shipState As System.String = String.Empty
	Private Dim _shipZip As System.String = String.Empty
	Private Dim _shipCountry As System.String = String.Empty
	Private Dim _billAddr1 As System.String = String.Empty
	Private Dim _billAddr2 As System.String = String.Empty
	Private Dim _billCity As System.String = String.Empty
	Private Dim _billState As System.String = String.Empty
	Private Dim _billZip As System.String = String.Empty
	Private Dim _billCountry As System.String = String.Empty
	Private Dim _courier As System.String = String.Empty
	Private Dim _totalPrice As System.Decimal
	Private Dim _billToFirstName As System.String = String.Empty
	Private Dim _billToLastName As System.String = String.Empty
	Private Dim _shipToFirstName As System.String = String.Empty
	Private Dim _shipToLastName As System.String = String.Empty
	Private Dim _authorizationNumber As System.Int32
	Private Dim _locale As System.String = String.Empty

    Public Sub New()
    End Sub

    Public Sub New(ByVal id As System.Int32)
        Dim sql As New SqlService()
        sql.AddParameter("@OrderId", SqlDbType.VarChar, id)
        Dim reader As SqlDataReader = sql.ExecuteSqlReader("SELECT * FROM Orders WHERE OrderId = @OrderId")

        If reader.Read() Then
            Me.LoadFromReader(reader)
            reader.Close()
        Else
            If Not reader.IsClosed Then
                reader.Close()
            End If
            Throw New ApplicationException("Orders does not exist.")
        End If
    End Sub

    Public Sub New(ByVal reader As SqlDataReader)
        Me.LoadFromReader(reader)
    End Sub

    Protected Sub LoadFromReader(ByVal reader As SqlDataReader)
        If Not IsNothing(reader) AndAlso Not reader.IsClosed Then
            _id = reader.GetInt32(0)
			If Not reader.IsDBNull(1) Then
                _userId = reader.GetString(1)
            End If
			If Not reader.IsDBNull(2) Then
                _orderDate = reader.GetDateTime(2)
            End If
			If Not reader.IsDBNull(3) Then
                _shipAddr1 = reader.GetString(3)
            End If
			If Not reader.IsDBNull(4) Then
                _shipAddr2 = reader.GetString(4)
            End If
			If Not reader.IsDBNull(5) Then
                _shipCity = reader.GetString(5)
            End If
			If Not reader.IsDBNull(6) Then
                _shipState = reader.GetString(6)
            End If
			If Not reader.IsDBNull(7) Then
                _shipZip = reader.GetString(7)
            End If
			If Not reader.IsDBNull(8) Then
                _shipCountry = reader.GetString(8)
            End If
			If Not reader.IsDBNull(9) Then
                _billAddr1 = reader.GetString(9)
            End If
			If Not reader.IsDBNull(10) Then
                _billAddr2 = reader.GetString(10)
            End If
			If Not reader.IsDBNull(11) Then
                _billCity = reader.GetString(11)
            End If
			If Not reader.IsDBNull(12) Then
                _billState = reader.GetString(12)
            End If
			If Not reader.IsDBNull(13) Then
                _billZip = reader.GetString(13)
            End If
			If Not reader.IsDBNull(14) Then
                _billCountry = reader.GetString(14)
            End If
			If Not reader.IsDBNull(15) Then
                _courier = reader.GetString(15)
            End If
			If Not reader.IsDBNull(16) Then
                _totalPrice = reader.GetDecimal(16)
            End If
			If Not reader.IsDBNull(17) Then
                _billToFirstName = reader.GetString(17)
            End If
			If Not reader.IsDBNull(18) Then
                _billToLastName = reader.GetString(18)
            End If
			If Not reader.IsDBNull(19) Then
                _shipToFirstName = reader.GetString(19)
            End If
			If Not reader.IsDBNull(20) Then
                _shipToLastName = reader.GetString(20)
            End If
			If Not reader.IsDBNull(21) Then
                _authorizationNumber = reader.GetInt32(21)
            End If
			If Not reader.IsDBNull(22) Then
                _locale = reader.GetString(22)
            End If
        End If
    End Sub

    Public Sub Delete()
        Order.Delete(Id)
    End Sub

    Public Sub Update()
        Dim sql As New SqlService()
        Dim queryParameters As New StringBuilder()

        sql.AddParameter("@OrderId", SqlDbType.Int, Id)
        queryParameters.Append("OrderId = @OrderId")

		sql.AddParameter("@UserId", SqlDbType.VarChar, UserId)
        queryParameters.Append(", UserId = @UserId")
		sql.AddParameter("@OrderDate", SqlDbType.DateTime, OrderDate)
        queryParameters.Append(", OrderDate = @OrderDate")
		sql.AddParameter("@ShipAddr1", SqlDbType.VarChar, ShipAddr1)
        queryParameters.Append(", ShipAddr1 = @ShipAddr1")
		sql.AddParameter("@ShipAddr2", SqlDbType.VarChar, ShipAddr2)
        queryParameters.Append(", ShipAddr2 = @ShipAddr2")
		sql.AddParameter("@ShipCity", SqlDbType.VarChar, ShipCity)
        queryParameters.Append(", ShipCity = @ShipCity")
		sql.AddParameter("@ShipState", SqlDbType.VarChar, ShipState)
        queryParameters.Append(", ShipState = @ShipState")
		sql.AddParameter("@ShipZip", SqlDbType.VarChar, ShipZip)
        queryParameters.Append(", ShipZip = @ShipZip")
		sql.AddParameter("@ShipCountry", SqlDbType.VarChar, ShipCountry)
        queryParameters.Append(", ShipCountry = @ShipCountry")
		sql.AddParameter("@BillAddr1", SqlDbType.VarChar, BillAddr1)
        queryParameters.Append(", BillAddr1 = @BillAddr1")
		sql.AddParameter("@BillAddr2", SqlDbType.VarChar, BillAddr2)
        queryParameters.Append(", BillAddr2 = @BillAddr2")
		sql.AddParameter("@BillCity", SqlDbType.VarChar, BillCity)
        queryParameters.Append(", BillCity = @BillCity")
		sql.AddParameter("@BillState", SqlDbType.VarChar, BillState)
        queryParameters.Append(", BillState = @BillState")
		sql.AddParameter("@BillZip", SqlDbType.VarChar, BillZip)
        queryParameters.Append(", BillZip = @BillZip")
		sql.AddParameter("@BillCountry", SqlDbType.VarChar, BillCountry)
        queryParameters.Append(", BillCountry = @BillCountry")
		sql.AddParameter("@Courier", SqlDbType.VarChar, Courier)
        queryParameters.Append(", Courier = @Courier")
		sql.AddParameter("@TotalPrice", SqlDbType.Decimal, TotalPrice)
        queryParameters.Append(", TotalPrice = @TotalPrice")
		sql.AddParameter("@BillToFirstName", SqlDbType.VarChar, BillToFirstName)
        queryParameters.Append(", BillToFirstName = @BillToFirstName")
		sql.AddParameter("@BillToLastName", SqlDbType.VarChar, BillToLastName)
        queryParameters.Append(", BillToLastName = @BillToLastName")
		sql.AddParameter("@ShipToFirstName", SqlDbType.VarChar, ShipToFirstName)
        queryParameters.Append(", ShipToFirstName = @ShipToFirstName")
		sql.AddParameter("@ShipToLastName", SqlDbType.VarChar, ShipToLastName)
        queryParameters.Append(", ShipToLastName = @ShipToLastName")
		sql.AddParameter("@AuthorizationNumber", SqlDbType.Int, AuthorizationNumber)
        queryParameters.Append(", AuthorizationNumber = @AuthorizationNumber")
		sql.AddParameter("@Locale", SqlDbType.VarChar, Locale)
        queryParameters.Append(", Locale = @Locale")

        Dim query As String = [String].Format("Update Orders Set {0} Where OrderId = @OrderId", queryParameters.ToString())
        Dim reader As SqlDataReader = sql.ExecuteSqlReader(query)
    End Sub

    Public Sub Create()
        Dim sql As New SqlService()
        Dim queryParameters As New StringBuilder()

        sql.AddParameter("@OrderId", SqlDbType.Int, Id)
        queryParameters.Append("@OrderId")

		sql.AddParameter("@UserId", SqlDbType.VarChar, UserId)
        queryParameters.Append(", @UserId")
		sql.AddParameter("@OrderDate", SqlDbType.DateTime, OrderDate)
        queryParameters.Append(", @OrderDate")
		sql.AddParameter("@ShipAddr1", SqlDbType.VarChar, ShipAddr1)
        queryParameters.Append(", @ShipAddr1")
		sql.AddParameter("@ShipAddr2", SqlDbType.VarChar, ShipAddr2)
        queryParameters.Append(", @ShipAddr2")
		sql.AddParameter("@ShipCity", SqlDbType.VarChar, ShipCity)
        queryParameters.Append(", @ShipCity")
		sql.AddParameter("@ShipState", SqlDbType.VarChar, ShipState)
        queryParameters.Append(", @ShipState")
		sql.AddParameter("@ShipZip", SqlDbType.VarChar, ShipZip)
        queryParameters.Append(", @ShipZip")
		sql.AddParameter("@ShipCountry", SqlDbType.VarChar, ShipCountry)
        queryParameters.Append(", @ShipCountry")
		sql.AddParameter("@BillAddr1", SqlDbType.VarChar, BillAddr1)
        queryParameters.Append(", @BillAddr1")
		sql.AddParameter("@BillAddr2", SqlDbType.VarChar, BillAddr2)
        queryParameters.Append(", @BillAddr2")
		sql.AddParameter("@BillCity", SqlDbType.VarChar, BillCity)
        queryParameters.Append(", @BillCity")
		sql.AddParameter("@BillState", SqlDbType.VarChar, BillState)
        queryParameters.Append(", @BillState")
		sql.AddParameter("@BillZip", SqlDbType.VarChar, BillZip)
        queryParameters.Append(", @BillZip")
		sql.AddParameter("@BillCountry", SqlDbType.VarChar, BillCountry)
        queryParameters.Append(", @BillCountry")
		sql.AddParameter("@Courier", SqlDbType.VarChar, Courier)
        queryParameters.Append(", @Courier")
		sql.AddParameter("@TotalPrice", SqlDbType.Decimal, TotalPrice)
        queryParameters.Append(", @TotalPrice")
		sql.AddParameter("@BillToFirstName", SqlDbType.VarChar, BillToFirstName)
        queryParameters.Append(", @BillToFirstName")
		sql.AddParameter("@BillToLastName", SqlDbType.VarChar, BillToLastName)
        queryParameters.Append(", @BillToLastName")
		sql.AddParameter("@ShipToFirstName", SqlDbType.VarChar, ShipToFirstName)
        queryParameters.Append(", @ShipToFirstName")
		sql.AddParameter("@ShipToLastName", SqlDbType.VarChar, ShipToLastName)
        queryParameters.Append(", @ShipToLastName")
		sql.AddParameter("@AuthorizationNumber", SqlDbType.Int, AuthorizationNumber)
        queryParameters.Append(", @AuthorizationNumber")
		sql.AddParameter("@Locale", SqlDbType.VarChar, Locale)
        queryParameters.Append(", @Locale")

        Dim query As String = [String].Format("Insert Into Orders ({0}) Values ({1})", queryParameters.ToString().Replace("@", ""), queryParameters.ToString())
        Dim reader As SqlDataReader = sql.ExecuteSqlReader(query)
    End Sub

    Public Shared Function NewOrder(ByVal id As System.Int32) As Order
        Dim newEntity As New Order()
        newEntity._id = id

        Return newEntity
    End Function

#Region "Public Properties"
    Public Property Id() As  System.Int32
        Get
            Return _id
        End Get
		Set(ByVal value As System.Int32)
            _id = value
        End Set
    End Property
	
	Public Property UserId() As System.String
        Get
            Return _userId
        End Get
        Set(ByVal value As System.String)
            _userId = value
        End Set
    End Property
	
	Public Property OrderDate() As System.DateTime
        Get
            Return _orderDate
        End Get
        Set(ByVal value As System.DateTime)
            _orderDate = value
        End Set
    End Property
	
	Public Property ShipAddr1() As System.String
        Get
            Return _shipAddr1
        End Get
        Set(ByVal value As System.String)
            _shipAddr1 = value
        End Set
    End Property
	
	Public Property ShipAddr2() As System.String
        Get
            Return _shipAddr2
        End Get
        Set(ByVal value As System.String)
            _shipAddr2 = value
        End Set
    End Property
	
	Public Property ShipCity() As System.String
        Get
            Return _shipCity
        End Get
        Set(ByVal value As System.String)
            _shipCity = value
        End Set
    End Property
	
	Public Property ShipState() As System.String
        Get
            Return _shipState
        End Get
        Set(ByVal value As System.String)
            _shipState = value
        End Set
    End Property
	
	Public Property ShipZip() As System.String
        Get
            Return _shipZip
        End Get
        Set(ByVal value As System.String)
            _shipZip = value
        End Set
    End Property
	
	Public Property ShipCountry() As System.String
        Get
            Return _shipCountry
        End Get
        Set(ByVal value As System.String)
            _shipCountry = value
        End Set
    End Property
	
	Public Property BillAddr1() As System.String
        Get
            Return _billAddr1
        End Get
        Set(ByVal value As System.String)
            _billAddr1 = value
        End Set
    End Property
	
	Public Property BillAddr2() As System.String
        Get
            Return _billAddr2
        End Get
        Set(ByVal value As System.String)
            _billAddr2 = value
        End Set
    End Property
	
	Public Property BillCity() As System.String
        Get
            Return _billCity
        End Get
        Set(ByVal value As System.String)
            _billCity = value
        End Set
    End Property
	
	Public Property BillState() As System.String
        Get
            Return _billState
        End Get
        Set(ByVal value As System.String)
            _billState = value
        End Set
    End Property
	
	Public Property BillZip() As System.String
        Get
            Return _billZip
        End Get
        Set(ByVal value As System.String)
            _billZip = value
        End Set
    End Property
	
	Public Property BillCountry() As System.String
        Get
            Return _billCountry
        End Get
        Set(ByVal value As System.String)
            _billCountry = value
        End Set
    End Property
	
	Public Property Courier() As System.String
        Get
            Return _courier
        End Get
        Set(ByVal value As System.String)
            _courier = value
        End Set
    End Property
	
	Public Property TotalPrice() As System.Decimal
        Get
            Return _totalPrice
        End Get
        Set(ByVal value As System.Decimal)
            _totalPrice = value
        End Set
    End Property
	
	Public Property BillToFirstName() As System.String
        Get
            Return _billToFirstName
        End Get
        Set(ByVal value As System.String)
            _billToFirstName = value
        End Set
    End Property
	
	Public Property BillToLastName() As System.String
        Get
            Return _billToLastName
        End Get
        Set(ByVal value As System.String)
            _billToLastName = value
        End Set
    End Property
	
	Public Property ShipToFirstName() As System.String
        Get
            Return _shipToFirstName
        End Get
        Set(ByVal value As System.String)
            _shipToFirstName = value
        End Set
    End Property
	
	Public Property ShipToLastName() As System.String
        Get
            Return _shipToLastName
        End Get
        Set(ByVal value As System.String)
            _shipToLastName = value
        End Set
    End Property
	
	Public Property AuthorizationNumber() As System.Int32
        Get
            Return _authorizationNumber
        End Get
        Set(ByVal value As System.Int32)
            _authorizationNumber = value
        End Set
    End Property
	
	Public Property Locale() As System.String
        Get
            Return _locale
        End Get
        Set(ByVal value As System.String)
            _locale = value
        End Set
    End Property
	
#End Region

    Public Shared Function GetOrder(ByVal id As String) As Order
        Return New Order(id)
    End Function
	
	Public Shared Sub Delete(ByVal id As System.Int32)
        Dim sql As New SqlService()
        sql.AddParameter("@OrderId", SqlDbType.Int, id)

        Dim reader As SqlDataReader = sql.ExecuteSqlReader("Delete Orders Where OrderId = @OrderId")
    End Sub
	
End Class
#End Region
End Namespace

