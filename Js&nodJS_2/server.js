// Send to another user
// References:
// https://stackoverflow.com/questions/13364243/websocketserver-node-js-how-to-differentiate-clients

// import library
const express = require('express')
const ServerSocket = require('ws').Server   // 引用 Server
const url = require('url')  // it's a built-in Nodejs module

const PORT = 8080

// 建立 express 物件並用來監聽 8080 port
const server = express()
    .listen(PORT, () => console.log(`[Server] Listening on https://localhost:${PORT}`))

// 建立實體，透過 ServerSocket 開啟 WebSocket 的服務
const wss = new ServerSocket({ server })

// front page https://myhost:8080?userId=client01&receiveId=client02
// pass parameters directly in the client connection URL

// Connection opened
wss.on('connection', (ws, req) => {
    console.log('[Open connected]')
    const parameters = url.parse(req.url, true);
    let userId = parameters.query.userId;
    let receiverId = parameters.query.receiverId;

    // setup client.id
    ws.id = userId;
    // send message back to client
    ws.send(`[Client ${ws.id} is connected!]`)

    // Listen for messages from client
    ws.on('message', data => {
        console.log('[Message from client ${ws.id}] data: ', data)
        // Get receiver
        let receiver;
        wss.clients.forEach(client => {
            if (client.id == receiverId) {
                receiver = client;
                return
            }
        })
        // Send message
        if (receiver == undefined) {
            let msg = `Not find receiver userId:${receiverId}`;
            ws.send(msg);
        } else {
            receiver.send(`To ${receiverId}\n From ${ws.id}: ${data}`)
            ws.send(`message sent: ${data}`);
        }
    })

    // Connection closed
    ws.on('close', () => {
        console.log('[Close connected]')
    })
})