'Crypto Facilities Ltd REST API V3

'Copyright(c) 2016 Crypto Facilities

'Permission Is hereby granted, free Of charge, to any person obtaining
'a copy Of this software And associated documentation files (the "Software"),
'to deal in the Software without restriction, including without limitation
'the rights To use, copy, modify, merge, publish, distribute, sublicense,
'And/Or sell copies of the Software, And to permit persons to whom the
'Software Is furnished To Do so, subject to the following conditions:

'The above copyright notice And this permission notice shall be included
'in all copies Or substantial portions of the Software.

'THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
'IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER LIABILITY,
'WHETHER IN AN ACTION OF CONTRACT, TORT Or OTHERWISE, ARISING FROM, OUT OF Or
'IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE SOFTWARE.

Imports System.Collections.Specialized
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text

Namespace com.cryptofacilities.REST.v3
    Public Class CfApiMethods
        Private apiPath As String
        Private apiPublicKey As String
        Private apiPrivateKey As String
        Private checkCertificate As Boolean
        Private nonce As Integer

        Sub New(ByVal apiPath As String, ByVal apiPublicKey As String, ByVal apiPrivateKey As String, ByVal checkCertificate As Boolean)
            Me.apiPath = apiPath
            Me.apiPublicKey = apiPublicKey
            Me.apiPrivateKey = apiPrivateKey
            Me.checkCertificate = checkCertificate
            nonce = 0
        End Sub

        Sub New(ByVal apiPath As String, ByVal checkCertificate As Boolean)
            Me.New(apiPath, Nothing, Nothing, checkCertificate)
        End Sub

        ' Signs a message
        Private Function signMessage(ByVal endpoint As String, ByVal nonce As String, ByVal postData As String) As String
            'Step 1: concatenate postData, nonce + endpoint
            Dim message = postData & nonce & endpoint

            'Step 2 hash the result of step 1 with SHA256
            Dim hash256 = New SHA256Managed()
            Dim hash = hash256.ComputeHash(Encoding.UTF8.GetBytes(message))

            'Step 3 base64 decode apiPrivateKey
            Dim secretDecoded = (System.Convert.FromBase64String(apiPrivateKey))

            'Step 4 use result of step 3 to hash the resultof step 2 with HMAC-SHA512
            Dim HMACSHA512 = New HMACSHA512(secretDecoded)
            Dim hash2 = HMACSHA512.ComputeHash(hash)

            'Step 5 base64 encode the result of step 4 And return
            Return System.Convert.ToBase64String(hash2)
        End Function

        ' Creates a nonce
        Private Function createNonce() As String
            nonce += 1
            Dim timestamp = CLng(DateTime.UtcNow.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds)
            Return timestamp & nonce.ToString("D4")
        End Function

        ' Sends an HTTP request
        Private Function makeRequest(ByVal requestMethod As String, ByVal endpoint As String, ByVal postUrl As String, ByVal postBody As String) As String
            If Not checkCertificate Then
                ServicePointManager.ServerCertificateValidationCallback = Function(sender, cert, chain, sslPolicyErrors) True
            End If

            Using client As New WebClient()
                Dim url = apiPath & endpoint & "?" & postUrl

                'create authentication headers
                If apiPublicKey IsNot Nothing AndAlso apiPrivateKey IsNot Nothing Then
                    Dim nonce = createNonce()
                    Dim postData = postUrl & postBody
                    Dim signature = signMessage(endpoint, nonce, postData)
                    client.Headers.Add("APIKey", apiPublicKey)
                    client.Headers.Add("Nonce", nonce)
                    client.Headers.Add("Authent", signature)
                End If

                If requestMethod = "POST" Then
                    Dim parameters = New NameValueCollection()
                    Dim bodyArray = postBody.Split("&"c)
                    For Each pair In bodyArray
                        Dim splitPair = pair.Split("="c)
                        parameters.Add(splitPair(0), splitPair(1))
                    Next

                    Dim response = client.UploadValues(url, "POST", parameters)
                    Return Encoding.UTF8.GetString(response)
                Else
                    Return client.DownloadString(url)
                End If
            End Using
        End Function

        Private Function makeRequest(ByVal requestMethod As String, ByVal endpoint As String) As String
            Return makeRequest(requestMethod, endpoint, String.Empty, String.Empty)
        End Function


        '----------------------------Public Endpoints------------------------------

        ' Returns all instruments with specifications
        Public Function getInstruments() As String
            Dim endpoint = "/api/v3/instruments"
            Return makeRequest("GET", endpoint)
        End Function


        ' Returns market data for all instruments
        Public Function getTickers() As String
            Dim endpoint = "/api/v3/tickers"
            Return makeRequest("GET", endpoint)
        End Function

        ' Returns the entire order book for a futures
        Public Function getOrderBook(ByVal symbol As String) As String
            Dim endpoint = "/api/v3/orderbook"
            Dim postUrl = "symbol=" + symbol
            Return makeRequest("GET", endpoint, postUrl, String.Empty)
        End Function

        ' Returns historical data for futures And indices
        Public Function getHistory(ByVal symbol As String, ByVal lastTime As DateTime) As String
            Dim endpoint = "/api/v3/history"
            Dim postUrl = "symbol=" & symbol & "&lastTime=" & lastTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            Return makeRequest("GET", endpoint, postUrl, String.Empty)
        End Function

        ' Returns historical data for futures And indices
        Public Function getHistory(ByVal symbol As String) As String
            Dim endpoint = "/api/v3/history"
            Dim postUrl = "symbol=" & symbol
            Return makeRequest("GET", endpoint, postUrl, String.Empty)
        End Function

        '----------------------------Private Endpoints------------------------------
       
        ' Returns key account information
        ' Deprecated because it returns info about the Futures margin account only
        <Obsolete("getAccount is deprecated, please use getAccounts instead.")>
        Public Function getAccount() As String
            Dim endpoint = "/api/v3/account"
            Return makeRequest("GET", endpoint)
        End Function

        ' Returns key account information
        Public Function getAccounts() As String
            Dim endpoint = "/api/v3/accounts"
            Return makeRequest("GET", endpoint)
        End Function

        ' Places an order
        Public Function sendOrder(ByVal orderType As String, ByVal symbol As String, ByVal side As String, ByVal size As Decimal, ByVal limitPrice As Decimal, Optional ByVal stopPrice As Decimal = 0D) As String
            Dim endpoint = "/api/v3/sendorder"
            Dim postBody As String
            If orderType = "lmt" Then
                postBody = String.Format("orderType=lmt&symbol={0}&side={1}&size={2}&limitPrice={3}", symbol, side, size, limitPrice)
            ElseIf orderType = "stp" Then
                postBody = String.Format("orderType=stp&symbol={0}&side={1}&size={2}&limitPrice={3}&stopPrice={4}", symbol, side, size, limitPrice, stopPrice)
            Else
                postBody = String.Empty
            End If
            Return makeRequest("POST", endpoint, String.Empty, postBody)
        End Function

        ' Cancels an order
        Public Function cancelOrder(ByVal orderId As String) As String
            Dim endpoint = "/api/v3/cancelorder"
            Dim postBody = "order_id=" & orderId
            Return makeRequest("POST", endpoint, String.Empty, postBody)
        End Function

        ' Places Or cancels orders in batch
        Public Function sendBatchOrder(ByVal jsonElement As String) As String
            Dim endpoint = "/api/v3/batchorder"
            Dim postBody = "json=" & jsonElement
            Return makeRequest("POST", endpoint, String.Empty, postBody)
        End Function

        ' Returns all open orders
        Public Function getOpenOrders() As String
            Dim endpoint = "/api/v3/openorders"
            Return makeRequest("GET", endpoint)
        End Function

        ' Returns filled orders
        Public Function getFills(ByVal lastFillTime As DateTime) As String
            Dim endpoint = "/api/v3/fills"
            Dim postUrl = "lastFillTime=" & lastFillTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            Return makeRequest("GET", endpoint, postUrl, String.Empty)
        End Function

        ' Returns filled orders
        Public Function getFills() As String
            Dim endpoint = "/api/v3/fills"
            Return makeRequest("GET", endpoint, String.Empty, String.Empty)
        End Function

        ' Returns all open positions
        Public Function getOpenPositions() As String
            Dim endpoint = "/api/v3/openpositions"
            Return makeRequest("GET", endpoint)
        End Function

        ' Sends an xbt witdrawal request
        Public Function sendWithdrawal(ByVal targetAddress As String, ByVal currency As String, ByVal amount As Decimal) As String
            Dim endpoint = "/api/v3/withdrawal"
            Dim postBody = String.Format("targetAddress={0}&currency={1}&amount={2}", targetAddress, currency, amount)
            Return makeRequest("POST", endpoint, String.Empty, postBody)
        End Function

        ' Returns xbt transfers
        Public Function getTransfers(ByVal lastTransferTime As DateTime) As String
            Dim endpoint = "/api/v3/transfers"
            Dim postUrl = "lastTransferTime=" & lastTransferTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            Return makeRequest("GET", endpoint, postUrl, String.Empty)
        End Function

        ' Returns xbt transfers
        Public Function getTransfers() As String
            Dim endpoint = "/api/v3/transfers"
            Return makeRequest("GET", endpoint)
        End Function

    End Class
End Namespace

