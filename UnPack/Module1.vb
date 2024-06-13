'https://www.clbgamesvn.com/diendan/showthread.php?t=314825
Imports System
Imports System.Collections
Imports System.IO
Imports System.IO.Compression
Imports System.Linq.Expressions
Imports System.Runtime
Imports System.Text
Imports System.Text.RegularExpressions
Imports UclCompression

Module Program

    Public br As BinaryReader
    Public input As String

    <Obsolete>
    Sub Main(args As String())
        If args.Count = 0 Then
            Console.WriteLine("Tool UnPack - 2CongLC.vn :: 2024")
        Else
            input = args(0)
        End If
        Dim p As String = Nothing
        If IO.File.Exists(input) Then

            br = New BinaryReader(File.OpenRead(input))
            Dim signature As String = New String(br.ReadChars(4)) ' Offset = 0, Length = 4
            Dim count As Int32 = br.ReadInt32 ' Offset = 4, Length = 4
            Dim index As Int32 = br.ReadInt32 ' Offset = 8, Length = 4
            Dim data As Int32 = br.ReadInt32 ' Offset = 12, Length = 4
            Dim crc32 As Int32 = br.ReadInt32 ' Offset = 16, Length = 4
            Dim reserved As Byte() = br.ReadBytes(12) 'Offset = 20, Length = 12

            Console.WriteLine("sig : {0}", signature)
            Console.WriteLine("count : {0}", count)
            Console.WriteLine("index : {0}", index)
            Console.WriteLine("data : {0}", data)
            Console.WriteLine("crc32 : {0}", crc32)

            br.BaseStream.Seek(index, SeekOrigin.Begin) 'br.BaseStream.Position = index
            Dim subfiles As New List(Of FileData)()
            For i As Int32 = 0 To count - 1
                subfiles.Add(New FileData)
            Next

            p = Path.GetDirectoryName(input) & "\" & Path.GetFileNameWithoutExtension(input)
            Directory.CreateDirectory(p)

            For Each fd As FileData In subfiles
                Console.WriteLine("File ID : {0} - File Offset : {1} - File Size : {2} - UncompressSize {3} - IsCompress : {4}", fd.id, fd.offset, fd.size, fd.uncompressSize, fd.isCompress)

                br.BaseStream.Position = fd.offset

                Dim buffer As Byte() = br.ReadBytes(fd.size)
                Dim ext As String = GetExtension(buffer)
                Dim unsize As UInt32 = CUInt(buffer(2)) << 16 Or CUInt(buffer(1)) << 8 Or CUInt(buffer(0))
                Dim temp As Byte() = Nothing

                If fd.isCompressType = 1 Or fd.isCompressType = 32 Then
                    temp = Ucl.NRV2B_Decompress_8(buffer, unsize)
                Else
                    temp = buffer
                End If

                Using bw As New BinaryWriter(File.Create(p & "//" & fd.id & ext))
                    bw.Write(temp)
                End Using
            Next

            Console.WriteLine("unpack done!!!")
        End If
        Console.ReadLine()
    End Sub

    Class FileData
        Public id As Int32 'Length = 4
        Public offset As Int32 'Length = 4
        Public size As Int32 'Length = 4
        Public unknow As Byte() 'Length = 3
        Public isCompresType As Byte 'Length = 1     
        Public Sub New()
            id = br.ReadInt32
            offset = br.ReadInt32
            size = br.ReadInt32
            unknow = br.ReadBytes(3)
            isCompressType = br.ReadByte
        End Sub
    End Class


    Public Function Hash(ByVal fileName As String) As UInt32
        Dim id As UInt32 = 0
        Dim index As Int32 = 0
        For Each c As Char In fileName
            If (AscW(c) >= AscW("A")) AndAlso (AscW(c) <= AscW("Z")) Then
                id = (id + (++index) * (AscW(c) + AscW("a") - AscW("A"))) Mod &H8000000BUI * &HFFFFFFEF
            Else
                id = (id + (++index) * AscW(c)) Mod &H8000000BUI * &HFFFFFFEF
            End If
        Next
        Return (id Xor &H12345678)
    End Function

End Module
