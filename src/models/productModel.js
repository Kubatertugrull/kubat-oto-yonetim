const db = require('../config/db');

class Product {
    static async getAll() {
        const [rows] = await db.execute('SELECT * FROM Urunler');
        return rows;
    }

    static async getById(id) {
        const [rows] = await db.execute('SELECT * FROM Urunler WHERE UrunID = ?', [id]);
        return rows[0];
    }

    static async updateStock(id, newStock) {
        await db.execute('UPDATE Urunler SET StokMiktari = ? WHERE UrunID = ?', [newStock, id]);
    }
}

module.exports = Product;
