Public Class AddItem

    ' used to tell the main window that the operation is cancelled
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
    End Sub

    ' used to insert the item
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        ' perform validation for barcode
        If TextBox1.Text.Trim = "" Then
            MsgBox("You should enter a barcode number", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox1.Focus()
            Exit Sub
        End If
        If Not IsNumeric(TextBox1.Text) Then
            MsgBox("The barcode number should include digits only", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox1.Focus()
            Exit Sub
        End If
        If TextBox1.Text.Contains(".") Or TextBox1.Text.Contains("-") Then
            MsgBox("The barcode number should include digits only", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox1.Focus()
            Exit Sub
        End If

        ' perform check for the item name
        If TextBox2.Text.Trim = "" Then
            MsgBox("You should enter name for the item", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox2.Focus()
            Exit Sub
        End If

        ' perform a check for the buy price
        If Not IsNumeric(TextBox3.Text) Then
            MsgBox("You should enter the buy price as a number", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox3.Focus()
            Exit Sub
        End If
        Dim BuyPrice As Decimal = Decimal.Parse(TextBox3.Text)
        If BuyPrice < 0 Then
            MsgBox("Buy price can't be negative", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox3.Focus()
            Exit Sub
        End If

        ' perform a check for the sell price
        If Not IsNumeric(TextBox4.Text) Then
            MsgBox("You should enter the sell price as a number", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox4.Focus()
            Exit Sub
        End If
        Dim SellPrice As Decimal = Decimal.Parse(TextBox4.Text)
        If SellPrice < 0 Then
            MsgBox("Sell price can't be negative", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox4.Focus()
            Exit Sub
        End If
        If SellPrice <= BuyPrice Then
            MsgBox("Sell price can't be less than buy price", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            TextBox4.Focus()
            Exit Sub
        End If

        ' insert the item
        Try
            ' create the adapter
            Dim TA As New POSDSTableAdapters.ItemsTableAdapter

            ' insert the item
            TA.Insert(TextBox1.Text, TextBox2.Text, BuyPrice, SellPrice)

            ' close window and return ok
            Me.DialogResult = Windows.Forms.DialogResult.OK
        Catch ex As Exception

            ' display error message
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub
End Class