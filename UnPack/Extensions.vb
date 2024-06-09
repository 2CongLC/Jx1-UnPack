
Imports System
Imports System.ComponentModel
Imports System.Text
Imports System.Text.RegularExpressions

Module Extensions

#Region "Document 30/05/2024"

    Private Const DEFAULT_EXTENSION_BINARY As String = ".unknow"
    Private Const DEFAULT_EXTENSION_TEXT As String = ".txt"
    Private ReadOnly tgaHeader As Byte() = {&H0, &H0, &H2, &H0, &H0, &H0, &H0, &H0}
    Private ReadOnly ttfHeader As Byte() = {&H5F, &HF, &H3C, &HF5}
    Private ReadOnly cppContent As String() = {"#include", "namespace"}
    Private ReadOnly luaContent As String() = {"function", "end"}
    Private ReadOnly iniPattern As New Regex("(\[.+\]\r*\n*(.*=.*\r*\n*){0,}){1,}", RegexOptions.Compiled Or RegexOptions.Multiline)
    Private ReadOnly xmlPattern As New Regex("(<.*?/{0,1}>){1,}", RegexOptions.Compiled Or RegexOptions.Multiline)

    <Obsolete>
    Private Function GetEncodingFromHeader(ByVal content As Byte()) As Encoding
        Dim bom = content.Take(4).ToArray()
        If bom(0) = &H2B AndAlso bom(1) = &H2F AndAlso bom(2) = &H76 Then Return Encoding.UTF7
        If bom(0) = &HEF AndAlso bom(1) = &HBB AndAlso bom(2) = &HBF Then Return Encoding.UTF8
        If bom(0) = &HFF AndAlso bom(1) = &HFE Then Return Encoding.Unicode
        If bom(0) = &HFE AndAlso bom(1) = &HFF Then Return Encoding.BigEndianUnicode
        If bom(0) = 0 AndAlso bom(1) = 0 AndAlso bom(2) = 0 AndAlso bom(3) = &HFE Then Return Encoding.UTF32
        Return Nothing
    End Function

    <Obsolete>
    Private Function IsPlainText(ByVal content As Byte(), ByRef assumedEncoding As Encoding) As Boolean
        For Each b As Byte In content
            If b = &H0 Then
                Dim encoding As Encoding = GetEncodingFromHeader(content)
                assumedEncoding = encoding
                If encoding Is Nothing Then
                    Return False
                Else
                    Return True
                End If
            End If
        Next
        assumedEncoding = Encoding.Default
        Return True
    End Function


    Public Function IsJson(ByVal data As String) As Boolean
        data = data.Trim()
        If cppContent.Any(Function(i) data.Contains(i)) Then ' Prevent it from being recognized as CPP
            Return False
        End If
        If data.StartsWith("{") Then
            Return data.EndsWith("}")
        ElseIf data.StartsWith("[") Then
            Return data.EndsWith("]")
        End If
        Return False
    End Function

    Public Function IsIni(ByVal data As String) As Boolean
        Return data.StartsWith("[") AndAlso iniPattern.IsMatch(data)
    End Function

    Public Function IsXml(ByVal data As String) As Boolean
        Return (data(0) = "<" AndAlso data(1) = "?" AndAlso data(2) = "x" AndAlso data(3) = "m" AndAlso data(4) = "l") _
            OrElse (data(0) = "<" AndAlso data(1) = "!" AndAlso data(2) = "-" AndAlso data(3) = "-") _
            OrElse xmlPattern.IsMatch(data)
    End Function

    Public Function IsCpp(ByVal data As String) As Boolean
        For Each item In cppContent
            If data.Contains(item) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function IsLua(ByVal data As String) As Boolean
        For Each item In luaContent
            If data.Contains(item) Then
                Return True
            End If
        Next
        Return False
    End Function

#End Region

#Region "Compression 30/05/5024 at 13:00:00"

    ''' <summary>
    ''' Kiểm tra định dạng WinRar
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function IsRar(ByVal data As Byte()) As Boolean
        Return data(0) = &H52 AndAlso data(1) = &H61 AndAlso data(2) = &H72 AndAlso data(3) = &H21
    End Function
    ''' <summary>
    ''' Kiểm tra định dạng Zip
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function IsZip(ByVal data As Byte()) As Boolean
        Return data(0) = &H50 AndAlso data(1) = &H4B AndAlso data(2) = &H3 AndAlso data(3) = &H4
    End Function
    ''' <summary>
    ''' Kiểm tra định dạng Bzip2
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function IsBZip2(ByVal data As Byte()) As Boolean
        Return data(0) = CByte(Asc("B")) AndAlso data(1) = CByte(Asc("Z"))
    End Function
    ''' <summary>
    ''' Kiểm tra định dạng Lz
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function IsLZip(ByVal data As Byte()) As Boolean
        Return data(0) = CByte(Asc("L")) AndAlso data(1) = CByte(Asc("Z")) AndAlso data(2) = CByte(Asc("I")) AndAlso data(3) = CByte(Asc("P"))
    End Function
    ''' <summary>
    ''' Kiểm tra định dạng Xz
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function IsXz(ByVal data As Byte()) As Boolean
        Return data(0) = &HFD AndAlso data(1) = &H37 AndAlso data(2) = &H7A AndAlso data(3) = &H58 AndAlso data(4) = &H5A
    End Function
    Public Function IsGzip(ByVal data As Byte()) As Boolean
        Return data(0) = &H1F AndAlso data(1) = &H8B AndAlso data(2) = &H8
    End Function
    Public Function Islzma(ByVal data As Byte()) As Boolean
        Return data(0) = &H5D AndAlso data(3) = &H40
    End Function
    Public Function IsZlib(ByVal data As Byte()) As Boolean
        Return data(0) = &H78 AndAlso data(1) = &H9C
    End Function
    Public Function IsSnappy(ByVal data As Byte()) As Boolean
        Return data(4) = &H73 AndAlso data(5) = &H4E AndAlso data(6) = &H61 AndAlso data(7) = &H50 AndAlso data(8) = &H70 AndAlso data(9) = &H59
    End Function
    Public Function IsZstd(ByVal data As Byte()) As Boolean
        Return data(0) = &H28 AndAlso data(2) = &H2F AndAlso data(5) = &H58
    End Function
    Public Function IsUcl(ByVal data As Byte()) As Boolean
        Return data(0) = &HFB Or data(0) = &HFF
    End Function
#End Region

#Region "Media 30/05/2024 at 13:20:00"

    Public Function IsOgg(ByVal data As Byte()) As Boolean
        Return data(0) = &H4F AndAlso data(1) = &H67 AndAlso data(2) = &H67 AndAlso data(3) = &H53
    End Function

    Public Function IsWav(ByVal data As Byte()) As Boolean
        Return (data(0) = &H52 AndAlso data(1) = &H49 AndAlso data(2) = &H46 AndAlso data(3) = &H46) _
            AndAlso (data(8) = &H57 AndAlso data(9) = &H41 AndAlso data(10) = &H56 AndAlso data(11) = &H45)
    End Function

    Public Function IsFsb(ByVal data As Byte()) As Boolean
        Return data(0) = &H46 AndAlso data(1) = &H53 AndAlso data(2) = &H42 AndAlso data(3) = &H35
    End Function

    Public Function IsMp4(ByVal data As Byte()) As Boolean
        Return data(4) = &H66 AndAlso data(5) = &H74 AndAlso data(6) = &H79 AndAlso data(7) = &H70
    End Function

    Public Function IsAvi(ByVal data As Byte()) As Boolean
        Return (data(0) = &H52 AndAlso data(1) = &H49 AndAlso data(2) = &H46 AndAlso data(3) = &H46) _
            AndAlso (data(8) = &H41 AndAlso data(9) = &H56 AndAlso data(10) = &H49 AndAlso data(11) = &H20)
    End Function

    Private Function IsMask(data As UInteger, mask As UInteger) As Boolean
        Return (data And mask) = mask
    End Function

    Private Function SwitchEndian(value As UInteger) As UInteger
        Return (value And &HFFUI) << 24 Or (value And &HFF00UI) << 8 Or (value And &HFF0000UI) >> 8 Or (value And &HFF000000UI) >> 24
    End Function

    Private Function PatternIndexOf(source As Byte(), pattern As Byte()) As Integer
        For i As Integer = 0 To source.Length - 1
            If source.Skip(i).Take(pattern.Length).SequenceEqual(pattern) Then
                Return i
            End If
        Next
        Return -1
    End Function

    Public Function IsMp3(data As Byte()) As Boolean
        ' ID3V2
        ' "ID3", first 3 bytes
        If data(0) = AscW("I"c) AndAlso data(1) = AscW("D"c) AndAlso data(2) = AscW("3"c) Then
            Return True
        End If

        ' ID3V1
        ' "TAG", start of the last 128 bytes
        If data(data.Length - &H80) = AscW("T"c) AndAlso data(data.Length - &H81) = AscW("A"c) AndAlso data(data.Length - &H82) = AscW("G"c) Then
            Return True
        End If

        ' Raw data
        Dim head As UInteger = SwitchEndian(BitConverter.ToUInt32(data.Take(4).ToArray(), 0))
        ' Sync head (31~21bit all 1)
        If IsMask(head, &HFFE00000UI) Then
            ' MPEG-1 (20~19bit is 11)
            If IsMask(head, &H1A0000UI) Then
                ' Layer 3 (18~17bit is 01)
                If IsMask(head, &H20000UI) Then
                    Return True
                End If
            End If
        End If

        Return False
    End Function


#End Region

#Region "Image 30/05/2024"

    Public Function IsPng(ByVal data As Byte()) As Boolean
        Return data(0) = &H89 AndAlso data(1) = &H50 AndAlso data(2) = &H4E AndAlso data(3) = &H47
    End Function

    Public Function IsJpeg(ByVal data As Byte()) As Boolean
        Return (data(0) = &HFF AndAlso data(1) = &HD8) _
            AndAlso (data(data.Length - 2) = &HFF AndAlso data(data.Length - 1) = &HD9)
    End Function

    Public Function IsBmp(ByVal data As Byte()) As Boolean
        Return (data(0) = &H42 AndAlso data(1) = &H4D) _
            AndAlso (BitConverter.ToInt32(data.Skip(2).Take(4).ToArray(), 0) = data.Length)
    End Function

    Public Function IsTga(ByVal data As Byte()) As Boolean
        Return data(0) = &H0 AndAlso data(1) = &H0 AndAlso data(2) = &H2 AndAlso data(3) = &H0 _
            AndAlso data(4) = &H0 AndAlso data(5) = &H0 AndAlso data(6) = &H0 AndAlso data(7) = &H0
    End Function

    Public Function IsDds(ByVal data As Byte()) As Boolean
        Return data(0) = &H44 AndAlso data(1) = &H44 AndAlso data(2) = &H53 AndAlso data(3) = &H20
    End Function

    Public Function IsPsd(ByVal data As Byte()) As Boolean
        Return data(0) = &H38 AndAlso data(1) = &H42 AndAlso data(2) = &H50 AndAlso data(3) = &H53
    End Function

    Public Function IsIff(ByVal data As Byte()) As Boolean
        Return (data(0) = &H46 AndAlso data(1) = &H4F AndAlso data(2) = &H52 AndAlso data(3) = &H34) _
            AndAlso (data(8) = &H43 AndAlso data(9) = &H49 AndAlso data(10) = &H4D AndAlso data(11) = &H47)
    End Function


#End Region

#Region "Others"
    Public Function IsCur(ByVal data As Byte()) As Boolean
        Return data(0) = &H0 AndAlso data(1) = &H0 AndAlso data(2) = &H2 AndAlso data(3) = &H0
    End Function

    Public Function IsAni(ByVal data As Byte()) As Boolean
        Return (data(0) = &H52 AndAlso data(1) = &H49 AndAlso data(2) = &H46 AndAlso data(3) = &H46) _
            AndAlso (data(8) = &H41 AndAlso data(9) = &H43 AndAlso data(10) = &H4F AndAlso data(11) = &H4E)
    End Function

    Public Function IsIco(ByVal data As Byte()) As Boolean
        Return data(0) = &H0 AndAlso data(1) = &H0 AndAlso data(2) = &H1 AndAlso data(3) = &H0
    End Function

#End Region

    Private ReadOnly binaryFormats As New Dictionary(Of String, Func(Of Byte(), Boolean))() From {
            {".ani", AddressOf IsAni},
            {".avi", AddressOf IsAvi},
            {".bmp", AddressOf IsBmp},
            {".dds", AddressOf IsDds},
            {".fsb", AddressOf IsFsb},
            {".ico", AddressOf IsIco},
            {".iff", AddressOf IsIff},
            {".jpeg", AddressOf IsJpeg},
            {".mp3", AddressOf IsMp3},
            {".mp4", AddressOf IsMp4},
            {".ogg", AddressOf IsOgg},
            {".png", AddressOf IsPng},
            {".psd", AddressOf IsPsd},
            {".rar", AddressOf IsRar},
            {".tga", AddressOf IsTga},
            {".cur", AddressOf IsCur},
            {".wav", AddressOf IsWav},
            {".zip", AddressOf IsZip},
            {".bzip2", AddressOf IsBZip2},
            {".Gzip", AddressOf IsGzip},
            {".lzma", AddressOf Islzma},
            {".zlib", AddressOf IsZlib},
            {".Snappy", AddressOf IsSnappy},
            {".Zstd", AddressOf IsZstd},
            {".ucl", AddressOf IsUcl}
        }


    Private ReadOnly textFormats As New Dictionary(Of String, Func(Of String, Boolean))() From {
        {".json", AddressOf IsJson},
        {".xml", AddressOf IsXml},
        {".ini", AddressOf IsIni},
        {".cpp", AddressOf IsCpp},
        {".lua", AddressOf IsLua}
    }

    Private strBuf As String

    <Obsolete>
    Public Function GetExtension(ByVal data As Byte()) As String
        Dim strBuf As String
        Dim enc As Encoding = Nothing
        If IsPlainText(data, enc) Then
            strBuf = enc.GetString(data)
            For Each txtFmt In textFormats
                Try
                    If txtFmt.Value(strBuf) Then
                        Return txtFmt.Key
                    End If
                Catch
                End Try
            Next
            Return DEFAULT_EXTENSION_TEXT
        Else
            For Each binFmt In binaryFormats
                Try
                    If binFmt.Value(data) Then
                        Return binFmt.Key
                    End If
                Catch
                End Try
            Next
            Return DEFAULT_EXTENSION_BINARY
        End If
    End Function


End Module
