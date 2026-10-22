using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace miPrimeaAplicacion
{
    public partial class frm_usuario : Form
    {
        public frm_usuario()
        {
            InitializeComponent();
        }

        Conexion objConexion = new Conexion();
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        String accion = "nuevo";
        int posicion = 0;

        // Único método Load (el que el diseñador tiene conectado)
        private void frm_usuario_Load_1(object sender, EventArgs e)
        {
            try
            {
                activarDesactivarCtrls(false);
                obtenerDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los usuarios:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void obtenerDatos()
        {
            ds.Clear();
            ds = objConexion.obtenerDatos();

            // Validación por seguridad si la tabla no viene en el DataSet
            if (ds.Tables.Contains("usuarios"))
            {
                dt = ds.Tables["usuarios"];
                dt.PrimaryKey = new DataColumn[] { dt.Columns["idUsuario"] };
            }
            else if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0]; // Por si acaso usa otro nombre de tabla por defecto
            }

            if (posicion >= dt.Rows.Count) posicion = dt.Rows.Count - 1;
            if (posicion < 0) posicion = 0;

            mostrarDatos();
        }

        private void mostrarDatos()
        {
            if (dt.Rows.Count > 0)
            {
                lblIdUsuario.Text = dt.Rows[posicion]["idUsuario"].ToString();
                txtUsuario.Text = dt.Rows[posicion]["usuario"].ToString();
                txtClave.Text = dt.Rows[posicion]["clave"].ToString();
                txtConfirmarClave.Text = dt.Rows[posicion]["clave"].ToString();
                txtNombre.Text = dt.Rows[posicion]["nombre"].ToString();
                txtDireccion.Text = dt.Rows[posicion]["direccion"].ToString();
                txtTelefono.Text = dt.Rows[posicion]["telefono"].ToString();
                lblRegistros.Text = (posicion + 1) + " de " + dt.Rows.Count;
            }
            else
            {
                limpiarControles();
                lblRegistros.Text = "0 de 0";
            }
        }

        private void limpiarControles()
        {
            lblIdUsuario.Text = "";
            txtUsuario.Text = "";
            txtClave.Text = "";
            txtConfirmarClave.Text = "";
            txtNombre.Text = "";
            txtDireccion.Text = "";
            txtTelefono.Text = "";
        }

        // estado = true  -> campos editables
        // estado = false -> campos sombreados y de solo lectura
        private void activarDesactivarCtrls(Boolean estado)
        {
            grbDatos.Enabled = true;

            foreach (Control c in grbDatos.Controls)
            {
                if (c is TextBox txt)
                {
                    // El ID (lblIdUsuario) no es un TextBox, pero si usaras un txtId iría aquí.
                    txt.ReadOnly = !estado;
                    txt.BackColor = estado ? SystemColors.Window : SystemColors.Control;
                }
            }

            grbNavegacion.Enabled = !estado;
            btnEliminar.Enabled = !estado;
        }

        // Devuelve "" si todo está bien, o el mensaje del primer error encontrado
        private String validarDatos()
        {
            String usuario = txtUsuario.Text.Trim();

            // Usuario: mínimo 6 y máximo 16 caracteres
            if (usuario.Length < 6 || usuario.Length > 16)
            {
                return "El usuario debe tener como mínimo 6 y como máximo 16 caracteres.";
            }

            // Usuario repetido
            String idActual = (accion == "modificar") ? lblIdUsuario.Text : "";
            foreach (DataRow fila in dt.Rows)
            {
                bool mismoRegistro = fila["idUsuario"].ToString() == idActual;
                bool mismoNombre = string.Equals(fila["usuario"].ToString().Trim(), usuario,
                    StringComparison.OrdinalIgnoreCase);
                if (mismoNombre && !mismoRegistro)
                {
                    return "Ya existe un usuario con ese nombre.";
                }
            }

            // Clave: confirmación
            if (txtClave.Text != txtConfirmarClave.Text)
            {
                return "La clave y su confirmación no coinciden.";
            }

            // Clave: letras y números
            if (!txtClave.Text.Any(char.IsLetter) || !txtClave.Text.Any(char.IsDigit))
            {
                return "La clave debe contener letras y números.";
            }

            // Nombre obligatorio
            if (txtNombre.Text.Trim() == "")
            {
                return "Ingrese el nombre.";
            }

            // Teléfono: la columna admite como máximo 9 caracteres y validamos que sean números (opcional pero seguro)
            String telefono = txtTelefono.Text.Trim();
            if (telefono.Length > 9)
            {
                return "El teléfono no puede tener más de 9 caracteres.";
            }
            if (telefono.Length > 0 && !telefono.All(char.IsDigit))
            {
                return "El teléfono solo debe contener números.";
            }

            return "";
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (btnAgregar.Text == "Agregar")
            {
                btnAgregar.Text = "Guardar";
                btnModificar.Text = "Cancelar";
                accion = "nuevo";
                limpiarControles();
                activarDesactivarCtrls(true);
                txtUsuario.Focus();
            }
            else
            { // Guardar
                String error = validarDatos();
                if (error != "")
                {
                    MessageBox.Show(error, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // no guarda
                }

                // CORRECCIÓN CLAVE: Si es nuevo, mandamos "0" (o cadena vacía controlada) en lugar de lblIdUsuario.Text que está en blanco
                String idEnviar = (accion == "nuevo") ? "0" : lblIdUsuario.Text;

                String[] usuarios = {
                    idEnviar,
                    txtUsuario.Text.Trim(),
                    txtClave.Text,
                    txtNombre.Text.Trim(),
                    txtDireccion.Text.Trim(),
                    txtTelefono.Text.Trim()
                };

                String respuesta = objConexion.mantenimiento_usuarios_usss124226(usuarios, accion);
                if (respuesta != "1")
                {
                    MessageBox.Show(respuesta, "Error al guardar usuarios.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    bool eraNuevo = (accion == "nuevo");

                    activarDesactivarCtrls(false);
                    btnAgregar.Text = "Agregar";
                    btnModificar.Text = "Modificar";

                    if (eraNuevo)
                    {
                        posicion = int.MaxValue; // obtenerDatos() lo ajusta al último registro
                    }
                    obtenerDatos();

                    MessageBox.Show("Registro guardado correctamente.", "Usuarios",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (btnModificar.Text == "Modificar")
            {
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No hay registros para modificar.", "Usuarios",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                btnAgregar.Text = "Guardar";
                btnModificar.Text = "Cancelar";
                activarDesactivarCtrls(true);
                accion = "modificar";
                txtUsuario.Focus();
            }
            else
            { // Cancelar
                mostrarDatos();
                activarDesactivarCtrls(false);
                btnAgregar.Text = "Agregar";
                btnModificar.Text = "Modificar";
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay registros para eliminar.", "Usuarios",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar a " + txtNombre.Text + "?",
                "Eliminando usuarios", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                String respuesta = objConexion.mantenimiento_usuarios_usss124226(
                    new String[] { lblIdUsuario.Text, "", "", "", "", "" }, "eliminar");
                if (respuesta != "1")
                {
                    MessageBox.Show(respuesta, "Error al eliminar usuarios.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    posicion = 0;
                    obtenerDatos();
                }
            }
        }

        private void btnPrimero_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            posicion = 0;
            mostrarDatos();
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            if (posicion > 0)
            {
                posicion--;
                mostrarDatos();
            }
            else
            {
                MessageBox.Show("Estás en el primer registro.", "Navegación de usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            if (posicion < dt.Rows.Count - 1)
            {
                posicion++;
                mostrarDatos();
            }
            else
            {
                MessageBox.Show("Estás en el último registro.", "Navegación de usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnUltimo_Click(object sender, EventArgs e)
        {
            if (dt.Rows.Count == 0) return;
            posicion = dt.Rows.Count - 1;
            mostrarDatos();
        }
    }
}