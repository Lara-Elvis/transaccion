using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace transaccion
{
    public partial class Form1 : Form
    {
        private string connectionString = "User Id=Visel;Password=Conexion890;Data Source = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = orcl)));";


        public Form1()
        {
            InitializeComponent();
            LoadTipoTransaccion();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarTransacciones();
        }

        private void LoadTipoTransaccion()
        {
            
            cmbTipoTransaccion.Items.Add("Pago");
            cmbTipoTransaccion.Items.Add("Transferencia");
            cmbTipoTransaccion.Items.Add("Depósito");
            
        }

        

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Conexión a la base de datos establecida.");

                    decimal saldoDebito = 0;
                    decimal monto = Convert.ToDecimal(txtMonto.Text);

                    // Verificar cuenta de débito
                    string verificarCuentaDebitoQuery = "SELECT saldo FROM CuentasBancarias WHERE numero_cuenta = :numero_cuenta_debito";
                    using (OracleCommand cmd = new OracleCommand(verificarCuentaDebitoQuery, conn))
                    {
                        string cuentaDebito = txtNoCuentaDebit.Text.Trim(); // Eliminar espacios en blanco
                        MessageBox.Show("Número de cuenta de débito (después de Trim): " + cuentaDebito);

                        cmd.Parameters.Add(new OracleParameter("numero_cuenta_debito", cuentaDebito));

                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("La cuenta de débito no existe o el saldo no es válido.");
                            return;
                        }
                        saldoDebito = Convert.ToDecimal(result);
                        MessageBox.Show("Saldo de la cuenta de débito: " + saldoDebito);
                    }

                    if (saldoDebito < monto)
                    {
                        MessageBox.Show("Saldo insuficiente en la cuenta de débito.");
                        return;
                    }

                    // Verificar cuenta de crédito
                    string verificarCuentaCreditoQuery = "SELECT saldo FROM CuentasBancarias WHERE numero_cuenta = :numero_cuenta_credito";
                    using (OracleCommand cmd = new OracleCommand(verificarCuentaCreditoQuery, conn))
                    {
                        string cuentaCredito = txtNoCuentaCredito.Text.Trim(); // Eliminar espacios en blanco
                        MessageBox.Show("Número de cuenta de crédito (después de Trim): " + cuentaCredito);

                        cmd.Parameters.Add(new OracleParameter("numero_cuenta_credito", cuentaCredito));

                        object result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("La cuenta de crédito no existe o el saldo no es válido.");
                            return;
                        }
                        MessageBox.Show("La cuenta de crédito existe.");
                    }

                    // Realizar transacción
                    OracleTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        // Debitar cuenta de débito
                        string debitoQuery = "UPDATE CuentasBancarias SET saldo = saldo - :monto WHERE numero_cuenta = :numero_cuenta_debito";
                        using (OracleCommand cmd = new OracleCommand(debitoQuery, conn))
                        {
                            cmd.Transaction = transaction;
                            string cuentaDebito = txtNoCuentaDebit.Text.Trim(); // Reutilizar la variable con valor trimmed
                            cmd.Parameters.Add(new OracleParameter("monto", monto));
                            cmd.Parameters.Add(new OracleParameter("numero_cuenta_debito", cuentaDebito));
                            MessageBox.Show("Ejecutando débito: " + debitoQuery + " con monto: " + monto + " y número de cuenta: " + cuentaDebito);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Cuenta de débito actualizada.");
                        }

                        // Acreditar cuenta de crédito
                        string creditoQuery = "UPDATE CuentasBancarias SET saldo = saldo + :monto WHERE numero_cuenta = :numero_cuenta_credito";
                        using (OracleCommand cmd = new OracleCommand(creditoQuery, conn))
                        {
                            cmd.Transaction = transaction;
                            string cuentaCredito = txtNoCuentaCredito.Text.Trim(); // Reutilizar la variable con valor trimmed
                            cmd.Parameters.Add(new OracleParameter("monto", monto));
                            cmd.Parameters.Add(new OracleParameter("numero_cuenta_credito", cuentaCredito));
                            MessageBox.Show("Ejecutando crédito: " + creditoQuery + " con monto: " + monto + " y número de cuenta: " + cuentaCredito);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Cuenta de crédito actualizada.");
                        }

                        // Obtener IDs de las cuentas
                        int cuentaDebitoId = ObtenerCuentaIdPorNumero(conn, txtNoCuentaDebit.Text.Trim(), transaction);
                        int cuentaCreditoId = ObtenerCuentaIdPorNumero(conn, txtNoCuentaCredito.Text.Trim(), transaction);

                        // Registrar transacción
                        string transaccionQuery = "INSERT INTO Transacciones (tipo_transaccion, cuenta_debito_id, cuenta_credito_id, monto) " +
                                                  "VALUES (:tipo_transaccion, :cuenta_debito_id, :cuenta_credito_id, :monto)";
                        using (OracleCommand cmd = new OracleCommand(transaccionQuery, conn))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.Add(new OracleParameter("tipo_transaccion", cmbTipoTransaccion.SelectedItem.ToString()));
                            cmd.Parameters.Add(new OracleParameter("cuenta_debito_id", cuentaDebitoId));
                            cmd.Parameters.Add(new OracleParameter("cuenta_credito_id", cuentaCreditoId));
                            cmd.Parameters.Add(new OracleParameter("monto", monto));
                            MessageBox.Show("Insertando transacción: " + transaccionQuery);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Transacción registrada.");
                        }

                        transaction.Commit();
                        MessageBox.Show("Transacción realizada correctamente.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error al realizar la transacción: " + ex.Message);
                    }

                    CargarTransacciones();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al realizar la transacción: " + ex.Message);
                }
            }
        }

        private int ObtenerCuentaIdPorNumero(OracleConnection conn, string numeroCuenta, OracleTransaction transaction)
        {
            string query = "SELECT cuenta_id FROM CuentasBancarias WHERE numero_cuenta = :numero_cuenta";
            using (OracleCommand cmd = new OracleCommand(query, conn))
            {
                cmd.Transaction = transaction; 
                cmd.Parameters.Add(new OracleParameter("numero_cuenta", numeroCuenta));
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void CargarTransacciones()
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Transacciones";

                    using (OracleDataAdapter da = new OracleDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar las transacciones: " + ex.Message);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }


    }



}
