Option Strict On

Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Module WoonTotaal

    Class Api

        Private Url As String
        Private AccessToken As String
            
        Public Sub New(Url As String, ApiKey As String, Company As String, Username As String, Password As String)
            Me.Url = Url
            Me.AccessToken = GetAccessToken(ApiKey, Company, Username, Password)
        End Sub

        Private Function GetAccessToken(ApiKey As String, Company As String, Username As String, Password As String) As String 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/token")
            myReq.Method = "POST"
            myReq.ContentType = "application/json"
            myReq.Headers.add("Api-Key",ApiKey)
            Dim myData As String = "{""domainName"":""" & Company & """,""username"":""" & Username & """,""password"":""" & Password & """}"
            myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myreader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myreader.ReadToEnd
            Dim myParsedText As JObject = JObject.Parse(myText)
            Return CStr(myParsedText.SelectToken("$.access_token"))
        End Function

        Public Function GetListOfMaterials(SearchText as String) As List(Of String) 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Material/Browse")
            myReq.Method = "POST"
            myReq.ContentType = "application/json"
            myReq.Headers.add("Authorization", "bearer " & AccessToken)
            Dim myData As String = "{""filter"":{""description"":""" & SearchText & """}}"
            myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myReader.ReadToEnd
            Dim myParsedText As JObject = JObject.Parse(myText)
            Dim myModelNames As IEnumerable(Of JToken) = myParsedText.SelectTokens("$.items[*].description")
            Dim myModelStrings As List(Of String) = New List(Of String)
            For Each s as JToken In myModelNames
                myModelStrings.Add(Cstr(s))
            Next
            Return myModelStrings
        End Function

        Private Sub AddToListOfModels(myPrefix as String, myChildren as JArray, myModelStrings As List(Of String))
            For Each myObject as JObject In myChildren
                Dim nextChildren As JArray = JArray.FromObject(myObject.SelectToken("$.children"))
                Dim myDescription As String = CStr(myObject.SelectToken("$.description"))
                Dim nextPrefix As String = myPrefix & myDescription & ", "
                If nextChildren IsNot Nothing AndAlso nextChildren.Count()>0 Then
                    AddToListOfModels(nextPrefix,nextChildren,myModelStrings)
                Else
                    myModelStrings.Add(myPrefix & myDescription)
                End If
            Next
        End Sub

        Public Function GetListOfModels() As List(Of String) 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Model/GetAll")
            myReq.Headers.add("Authorization", "bearer " & AccessToken)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myReader.ReadToEnd
            Dim myChildren As JArray = JArray.Parse(myText)
            Dim myModelStrings As List(Of String) = New List(Of String)
            AddToListOfModels("",myChildren,myModelStrings)
            Return myModelStrings            
        End Function

    End Class

End Module
