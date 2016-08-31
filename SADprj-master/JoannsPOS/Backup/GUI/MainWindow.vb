Public Class MainWindow


    ' the form loads and initialization should happen
    Private Sub MainWindow_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Try


            ' get the password from the user
            Dim PSWWin As New PasswordPicker

            ' if the user hits the exit button then stop execution
            If PSWWin.ShowDialog <> Windows.Forms.DialogResult.OK Then
                End
            End If

            ' get the password
            Dim PSW As String = PSWWin.TextBox1.Text

            ' get the password from the database
            Dim TA As New POSDSTableAdapters.ValuesTableAdapter
            Dim TB = TA.GetDataByKey("password")
            Dim DBPSW As String = TB.Rows(0).Item(1)

            ' check that passwords match
            If PSW <> DBPSW Then
                MsgBox("invalid password", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
                End
            End If

            ' load the items information from db into the dataset
            ItemsTA.Fill(MyDataset.Items)

        Catch ex As Exception

            ' handle the error
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End
        End Try

    End Sub

    ' change the password
    Private Sub ChangePasswordToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        Dim PSWChange As New ChangePassword
        PSWChange.ShowDialog()
    End Sub

    ' add item to the db
    Private Sub AddItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddItemToolStripMenuItem.Click
        Dim AddItemWindow As New AddItem
        If AddItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' used to select an item
    Private Sub EditItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditItemToolStripMenuItem.Click

        ' make sure an item is selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode = DGV.SelectedRows(0).Cells(0).Value

        ' create the edit window
        Dim EditItemWindow As New EditItem

        ' fill the window with information
        EditItemWindow.FillItemInfo(Barcode)

        If EditItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' this one is used to remove an item
    Private Sub RemoveItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveItemToolStripMenuItem.Click

        ' make sure a single item is being selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode As String = DGV.SelectedRows(0).Cells(0).Value

        ' remove the item
        Try
            ItemsTA.DeleteByBarcode(Barcode)
            ItemsTA.Fill(MyDataset.Items)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' checks if the return key is pressed
    Private Sub TextBox1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If Button1.Enabled Then
            If e.KeyChar = Chr(13) Then
                Button1_Click(Nothing, Nothing)
            End If
        End If
    End Sub


    ' this one is used to detect the barcode item when the text change and display its information
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        Try
            ' step 01: create the table adapter
            Dim TA As New POSDSTableAdapters.ItemsTableAdapter
            Dim TB = TA.GetDataByBarcode(TextBox1.Text)

            ' step 02: check if no item is found
            If TB.Rows.Count = 0 Then
                TextBox2.Text = ""
                TextBox3.Text = ""
                Button1.Enabled = False
                Exit Sub
            End If

            ' step 03: display the information in the textboxes
            Button1.Enabled = True
            Dim R As POS.POSDS.ItemsRow = TB.Rows(0)
            TextBox2.Text = R.ItemName
            TextBox3.Text = R.SellPrice
            Button1.Tag = R
        Catch ex As Exception
            ' display error message
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' this will be used to add an item to the recipt details
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        ' get the details of the item
        Dim R As POS.POSDS.ItemsRow = Button1.Tag

        ' next search for the barcode in the datagridview
        Dim I As Integer
        Dim ItemLoc As Integer = -1
        For I = 0 To DGV2.Rows.Count - 1
            If R.Barcode = DGV2.Rows(I).Cells(0).Value Then

                ' item found
                ItemLoc = I
                Exit For

            End If
        Next

        ' if item is not found, add it
        If ItemLoc = -1 Then
            DGV2.Rows.Add(R.Barcode, R.ItemName, R.BuyPrice, R.SellPrice, 1, R.SellPrice)
        Else
            ' if item is already there increase its count
            Dim ItemCount As Long = DGV2.Rows(ItemLoc).Cells(4).Value
            ItemCount += 1
            Dim NewPrice As Decimal = R.SellPrice * ItemCount
            DGV2.Rows(ItemLoc).Cells(4).Value = ItemCount
            DGV2.Rows(ItemLoc).Cells(5).Value = NewPrice
        End If

        ' next clear textbox1 and set focus to it
        TextBox1.Text = ""
        TextBox1.Focus()

        ' compute the total for the recipt
        Dim Sum As Decimal = 0
        For I = 0 To DGV2.Rows.Count - 1
            Sum += DGV2.Rows(I).Cells(5).Value
        Next

        TextBox4.Text = Sum


    End Sub


    ' remove item from the recipt
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If DGV2.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        DGV2.Rows.Remove(DGV2.SelectedRows(0))
    End Sub


    ' save the recipt
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        Dim MyConnection As OleDb.OleDbConnection = Nothing
        Dim MyTransaction As OleDb.OleDbTransaction = Nothing

        Try

            ' create the connection and  transaction object
            myconnection = New OleDb.OleDbConnection(My.Settings.dbConnectionString)
            MyConnection.Open()
            MyTransaction = MyConnection.BeginTransaction

            ' insert the new recipt
            Dim SQL As String = "insert into recipts (reciptdate,recipttotal) values (:0,:1)"
            Dim CMD1 As New OleDb.OleDbCommand
            CMD1.Connection = MyConnection
            CMD1.Transaction = MyTransaction
            CMD1.CommandText = SQL
            CMD1.Parameters.AddWithValue(":0", Now.Date)
            CMD1.Parameters.AddWithValue(":1", TextBox4.Text)
            CMD1.ExecuteNonQuery()
            CMD1.Dispose()

            ' get the id for the recipt
            SQL = "select max(reciptid) as MAXID from recipts"
            Dim CMD2 As New OleDb.OleDbCommand
            CMD2.Connection = MyConnection
            CMD2.Transaction = MyTransaction
            CMD2.CommandText = SQL
            Dim ReciptID As Long = CMD2.ExecuteScalar()
            CMD2.Dispose()

            ' insert the details of the recipt
            Dim I As Integer
            For I = 0 To DGV2.Rows.Count - 1

                ' get the values
                Dim Barcode As String = DGV2.Rows(I).Cells(0).Value
                Dim BuyPrice As Decimal = DGV2.Rows(I).Cells(2).Value
                Dim SellPrice As Decimal = DGV2.Rows(I).Cells(3).Value
                Dim ItemCount As Integer = DGV2.Rows(I).Cells(4).Value

                ' next create a command
                Dim CMD3 As New OleDb.OleDbCommand
                SQL = "insert into ReciptDetails " & _
                      "(reciptid,barcode,itemcount,itembuyprice,itemsellprice) " & _
                      "values " & _
                      "(:0      ,:1     ,:2       ,:3          ,:4       )"
                CMD3.Connection = MyConnection
                CMD3.Transaction = MyTransaction
                CMD3.CommandText = SQL
                CMD3.Parameters.AddWithValue(":0", ReciptID)
                CMD3.Parameters.AddWithValue(":1", Barcode)
                CMD3.Parameters.AddWithValue(":2", ItemCount)
                CMD3.Parameters.AddWithValue(":3", BuyPrice)
                CMD3.Parameters.AddWithValue(":4", SellPrice)

                CMD3.ExecuteNonQuery()
                CMD3.Dispose()

            Next


            ' all well save the changes
            MyTransaction.Commit()

            ' close connection
            MyTransaction.Dispose()
            MyConnection.Close()
            MyConnection.Dispose()

            DGV2.Rows.Clear()
            TextBox4.Text = ""

        Catch ex As Exception
            If MyTransaction IsNot Nothing Then
                MyTransaction.Rollback()
            End If
            If myconnection IsNot Nothing Then
                If MyConnection.State = ConnectionState.Open Then
                    MyConnection.Close()
                End If
            End If

            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
      
    End Sub
End Class
