Option Strict On

Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Module Utils

    Class Config

        Private Values as JToken
            
        Public Sub New(FileName As String)
            Values = JObject.Parse(File.ReadAllText(FileName))
        End Sub

        Public Function Value(Key As String) As String
            Return CStr(Values.SelectToken("$." & Key))
        End Function

    End Class

End Module
