using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace miPrimeaAplicacion
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }
        Conexion objConexion = new Conexion();

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtUsuario.Text.Trim() == "" || txtClave.Text == "")
                {
                    MessageBox.Show("Ingrese usuario y clave.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataSet ds = objConexion.obtenerDatos();
                if (!ds.Tables.Contains("usuarios"))
                {
                    MessageBox.Show("No se encontró la tabla de usuarios en el DataSet. Revise obtenerDatos().", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool acceso = false;
                foreach (DataRow fila in ds.Tables["usuarios"].Rows)
                {
                    String usuarioBD = fila["usuario"].ToString().Trim();
                    String claveBD = fila["clave"].ToString().Trim();

                    if (usuarioBD == txtUsuario.Text.Trim() && claveBD == txtClave.Text)
                    {
                        acceso = true;
                        break;
                    }
                }

                if (acceso)
                {
                    this.DialogResult = DialogResult.OK; // cierra el login y pasa al principal
                }
                else
                {
                    MessageBox.Show("Usuario o clave incorrectos. Acceso denegado.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtClave.Clear();
                    txtClave.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error en el login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}