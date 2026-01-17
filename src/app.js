const express = require('express');
const app = express();
require('dotenv').config();

app.use(express.json());

const productRoutes = require('./routes/productRoutes');
app.use('/api/products', productRoutes);

app.get('/', (req, res) => {
    res.send('OtoParça API Çalışıyor');
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Sunucu ${PORT} portunda çalışıyor.`);
});
