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

        private void cmbTipoTransaccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            // llenar los campos basándose en el tipo de transacción seleccionado
            string tipoTransaccion = cmbTipoTransaccion.SelectedItem.ToString();

            if (tipoTransaccion == "Pago")
            {
                txtNoCuentaDebit.Text = "123456";
                txtNomCuentaDebit.Text = "Cuenta Debito Pago";
                txtNoCuentaCredito.Text = "654321";
                txtNomCuentaCredito.Text = "Cuenta Credito Pago";
                txtMonto.Text = "100.00";
            }
            else if (tipoTransaccion == "Transferencia")
            {
                txtNoCuentaDebit.Text = "234567";
                txtNomCuentaDebit.Text = "Cuenta Debito Transferencia";
                txtNoCuentaCredito.Text = "765432";
                txtNomCuentaCredito.Text = "Cuenta Credito Transferencia";
                txtMonto.Text = "200.00";
            }
            else if (tipoTransaccion == "Depósito")
            {
                txtNoCuentaDebit.Text = "345678";
                txtNomCuentaDebit.Text = "Cuenta Debito Depósito";
                txtNoCuentaCredito.Text = "876543";
                txtNomCuentaCredito.Text = "Cuenta Credito Depósito";
                txtMonto.Text = "300.00";
            }
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
                    string query = "INSERT INTO Transacciones (tipo_transaccion, no_cuenta_debit, nom_cuenta_debit, no_cuenta_credito, nom_cuenta_credito, monto) " +
                                   "VALUES (:tipo_transaccion, :no_cuenta_debit, :nom_cuenta_debit, :no_cuenta_credito, :nom_cuenta_credito, :monto)";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("tipo_transaccion", cmbTipoTransaccion.SelectedItem.ToString()));
                        cmd.Parameters.Add(new OracleParameter("no_cuenta_debit", txtNoCuentaDebit.Text));
                        cmd.Parameters.Add(new OracleParameter("nom_cuenta_debit", txtNomCuentaDebit.Text));
                        cmd.Parameters.Add(new OracleParameter("no_cuenta_credito", txtNoCuentaCredito.Text));
                        cmd.Parameters.Add(new OracleParameter("nom_cuenta_credito", txtNomCuentaCredito.Text));
                        cmd.Parameters.Add(new OracleParameter("monto", Convert.ToDecimal(txtMonto.Text)));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Transacción realizada correctamente.");
                    }
                    CargarTransacciones();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al realizar la transacción: " + ex.Message);
                }
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
