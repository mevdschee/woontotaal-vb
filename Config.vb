Option Strict On

Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Class Config

    Public Url As String
    Public ApiKey As String
    Public Company As String
    Public Username As String
    Public Password As String
        
    Public Sub New(FileName As String)
        Dim myText As String = File.ReadAllText(FileName)
        Dim myParsedText As JObject = JObject.Parse(myText)
        Url = CStr(myParsedText.SelectToken("$.Url"))
        ApiKey = CStr(myParsedText.SelectToken("$.ApiKey"))
        Company = CStr(myParsedText.SelectToken("$.Company"))
        Username = CStr(myParsedText.SelectToken("$.Username"))
        Password = CStr(myParsedText.SelectToken("$.Password"))
    End Sub

End Class
