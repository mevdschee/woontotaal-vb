Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Class WoonTotaal

    Private Url As String
    Private AccessToken As String
        
    Public Sub New(Url As String, ApiKey As String, Company As String, Username As String, Password As String)
        Me.Url = Url
        Me.AccessToken = GetAccessToken(ApiKey, Company, Username, Password)
    End Sub

    Private Function GetAccessToken(ApiKey As String, Company As String, Username As String, Password As String) As String 
        Dim myReq As HttpWebRequest = HttpWebRequest.Create(Url & "/token")
        myReq.Method = "POST"
        myReq.ContentType = "application/json"
        myReq.Headers.add("Api-Key",ApiKey)
        Dim myData As String = "{""domainName"":""" & Company & """,""username"":""" & Username & """,""password"":""" & Password & """}"
        myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
        Dim myResp As HttpWebResponse = myReq.GetResponse
        Dim myreader As New System.IO.StreamReader(myResp.GetResponseStream)
        Dim myText As String = myreader.ReadToEnd
        Dim myParsedText As JObject = JObject.Parse(myText)
        Return CStr(myParsedText.SelectToken("$.access_token"))
    End Function

    Public Function GetListOfMaterials(SearchText as String) As List(Of String) 
        Dim myReq As HttpWebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Material/Browse")
        myReq.Method = "POST"
        myReq.ContentType = "application/json"
        myReq.Headers.add("Authorization", "bearer " & AccessToken)
        Dim myData As String = "{""filter"":{""description"":""" & SearchText & """}}"
        myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
        Dim myResp As HttpWebResponse = myReq.GetResponse
        Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
        Dim myText As String = myReader.ReadToEnd
        Dim myParsedText As JObject = JObject.Parse(myText)
        Dim myMaterialNames As IEnumerable(Of JToken) = myParsedText.SelectTokens("$.items[*].description")
        Dim myMaterialStrings As List(Of String) = New List(Of String)
        For Each s In myMaterialNames
            myMaterialStrings.Add(s)
        Next
        Return myMaterialStrings
    End Function

End Class
