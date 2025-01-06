const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();

// Proxy configuration
app.use('/api', createProxyMiddleware({
    target: 'https://cobodex.eu', // Target server
    changeOrigin: true,
    pathRewrite: {
        '^/api': '', 
    },
}));

const port = 3001; // Port for the proxy server
app.listen(port, () => {
    console.log(`Proxy server running on port ${port}`);
});
