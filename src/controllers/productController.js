const Product = require('../models/productModel');

exports.getAllProducts = async (req, res) => {
    try {
        const products = await Product.getAll();
        res.status(200).json(products);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
};

exports.sellProduct = async (req, res) => {
    const { id } = req.params;
    const { quantity } = req.body;

    try {
        const product = await Product.getById(id);
        
        if (!product) {
            return res.status(404).json({ message: 'Ürün bulunamadı.' });
        }

        if (product.StokMiktari < quantity) {
            return res.status(400).json({ 
                message: 'Yetersiz Stok! Satış yapılamaz.',
                mevcutStok: product.StokMiktari
            });
        }

        const newStock = product.StokMiktari - quantity;
        await Product.updateStock(id, newStock);

        res.status(200).json({ 
            message: 'Satış başarılı.', 
            kalanStok: newStock 
        });

    } catch (error) {
        res.status(500).json({ error: error.message });
    }
};
