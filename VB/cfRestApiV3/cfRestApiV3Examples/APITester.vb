'Crypto Facilities Ltd REST API V3 Examples

'Copyright(c) 2018 Crypto Facilities

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


Imports cfRestApiV3.com.cryptofacilities.REST.v3

Namespace com.cryptofacilities.REST.v3.Examples

    Module APITester
        Private ReadOnly apiPath As String = "https://www.cryptofacilities.com/derivatives"
        Private ReadOnly apiPublicKey As String = "..." 'accessible On your Account page under Settings -> API Keys
        Private ReadOnly apiPrivateKey As String = "..." 'accessible On your Account page under Settings -> API Keys

        Private ReadOnly checkCertificate As Boolean = True 'When Using the test environment, this must be Set To "False"

        Sub Main()
            Dim methods As CfApiMethods
            Dim result, symbol, side, orderType As String
            Dim size, limitPrice, stopPrice As Decimal


            '---------------------------Public Endpoints-----------------------------------------------'
            methods = New CfApiMethods(apiPath, checkCertificate)

            'get instruments
            result = methods.getInstruments()
            Console.WriteLine("getInstruments:" & vbNewLine & result)

            'get tickers
            result = methods.getTickers()
            Console.WriteLine("getTickers:" & vbNewLine & result)

            'get orderbook
            symbol = "FI_XBTUSD_180316"
            result = methods.getOrderBook(symbol)
            Console.WriteLine("getOrderBook:" & vbNewLine & result)

            'get history
            symbol = "FI_XBTUSD_180316"
            result = methods.getHistory(symbol, New DateTime(2016, 01, 20))
            Console.WriteLine("getHistory:" & vbNewLine & result)

            '----------------------------Private Endpoints----------------------------------------------
            methods = New CfApiMethods(apiPath, apiPublicKey, apiPrivateKey, checkCertificate)

            'get accounts
            result = methods.getAccounts()
            Console.WriteLine("getAccounts:" & vbNewLine & result)

            'send limit order
            orderType = "lmt"
            symbol = "FI_XBTUSD_180316"
            side = "buy"
            size = 1D
            limitPrice = 1D
            result = methods.sendOrder(orderType, symbol, side, size, limitPrice)
            Console.WriteLine("sendOrder (limit):" & vbNewLine & result)

            'send stop order
            orderType = "stp"
            symbol = "FI_XBTUSD_180316"
            side = "buy"
            size = 1D
            limitPrice = 1.1D
            stopPrice = 2D
            result = methods.sendOrder(orderType, symbol, side, size, limitPrice, stopPrice)
            Console.WriteLine("sendOrder (stop):" & vbNewLine & result)

            'cancel order
            Dim orderId = "8a169b1d-0f36-495a-a641-539e7d24087e"
            result = methods.cancelOrder(orderId)
            Console.WriteLine("cancelOrder:" & vbNewLine & result)

            'batch order
            Dim jsonElement = "{
                ""batchOrder"":
                    [
                        {
                            ""order"": ""send"",
                            ""order_tag"": ""1"",
                            ""orderType"": ""lmt"",
                            ""symbol"": ""FI_XBTUSD_180316"",
                            ""side"": ""buy"",
                            ""size"": 1,
                            ""limitPrice"": 1.00,
                        },
                        {
                            ""order"": ""send"",
                            ""order_tag"": ""2"",
                            ""orderType"": ""stp"",
                            ""symbol"": ""FI_XBTUSD_180316"",             
                            ""side"": ""buy"",
                            ""size"": 1,
                            ""limitPrice"": 2.00,
                            ""stopPrice"": 3.00,
                        },
                        {
                            ""order"": ""cancel"",
                            ""order_id"": ""4fcc1de0-36cd-4309-9c34-7a81859cf221"",
                        },
                    ],
            }"
            result = methods.sendBatchOrder(jsonElement)
            Console.WriteLine("sendBatchOrder:" & vbNewLine & result)


            'get open orders
            result = methods.getOpenOrders()
            Console.WriteLine("getOpenOrders:" & vbNewLine & result)

            'get fills
            Dim lastFillTime = New DateTime(2016, 2, 1)
            result = methods.getFills(lastFillTime)
            Console.WriteLine("getFills:" & vbNewLine & result)

            'get open positions
            result = methods.getOpenPositions()
            Console.WriteLine("getOpenPositions:" & vbNewLine & result)

            'send xbt withdrawal request
            Dim targetAddress = "xxxxxxxxxxxxx"
            Dim currency = "xbt"
            Dim amount = 0.123D
            result = methods.sendWithdrawal(targetAddress, currency, amount)
            Console.WriteLine("sendWithdrawal:" & vbNewLine & result)

            'get xbt transfers
            Dim lastTransferTime = New DateTime(2016, 2, 1)
            result = methods.getTransfers(lastTransferTime)
            Console.WriteLine("getTransfers:" & vbNewLine & result)

            Console.ReadLine()
        End Sub

    End Module

End Namespace

