﻿Imports System.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.SecurityString

Public Module Globals

    Public Property Hash1 As String = getHashKey(GetType(App).Assembly)
    Public Property Hash2 As String = getHashKey(GetType(Globals).Assembly)
    Public ReadOnly Property byteOrder As ByteOrder = ByteOrderHelper.SystemByteOrder

    Sub New()

    End Sub

    Private Function getHashKey(assm As Assembly) As String
        Dim file As String = assm.Location.GetFullPath
        Dim md5 As String = SecurityString.GetFileMd5(file)

        Return md5.ToLower
    End Function

    Public Function EncryptData(random As Double, data As Byte()) As Byte()
        Dim salt = ByteOrderHelper.GetBytes(random)
        Dim key As String = GetHashKey64(salt)

        ' 进行data数据部分的加密
        Using encrypt As SecurityStringModel = New SHA256(key, salt)
            data = encrypt.Encrypt(data)
        End Using

        Dim buffer As Byte() = New Byte(salt.Length + data.Length - 1) {}

        Call Array.ConstrainedCopy(salt, Scan0, buffer, Scan0, salt.Length)
        Call Array.ConstrainedCopy(data, Scan0, buffer, salt.Length, data.Length)

        Return buffer
    End Function

    Public Function GetHashKey64(salt As Byte()) As String
        If salt(3) Mod 2 = 0 Then
            Return Globals.Hash1 & Globals.Hash2
        Else
            Return Globals.Hash2 & Globals.Hash1
        End If
    End Function

    Public Function GetRandomKey(buffer As Byte()) As Double
        Dim chunk8 As Byte() = New Byte(7) {}

        Call Array.ConstrainedCopy(buffer, Scan0, chunk8, Scan0, 8)

        If byteOrder = ByteOrder.LittleEndian Then
            Call Array.Reverse(chunk8)
        End If

        Return BitConverter.ToDouble(chunk8, Scan0)
    End Function

    Public Function DecryptData(data As Byte()) As Byte()
        Dim salt As Byte() = New Byte(7) {}
        Dim buffer As Byte() = New Byte(data.Length - 8) {}

        Call Array.ConstrainedCopy(data, Scan0, salt, Scan0, 8)
        Call Array.ConstrainedCopy(data, 8, buffer, Scan0, buffer.Length)

        Using encrypt As SecurityStringModel = New SHA256(GetHashKey64(salt), salt)
            data = encrypt.Decrypt(buffer)
        End Using

        Return data
    End Function

End Module
