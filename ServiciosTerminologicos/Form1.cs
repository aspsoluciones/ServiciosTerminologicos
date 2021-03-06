﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ServiciosTerminologicos.HIBA;

namespace ServiciosTerminologicos
{
    public partial class Form1 : Form
    {
        private TerminologiaWSClient ws = new TerminologiaWSClient();
        private long subsetIdInstitucion = 34701000999132; // id de cliente aspsoluciones

        private long dominioSustancias = 591000999139;         // SUSTANCIAS
        private long dominioProblemas = 601000999132;          // PROBLEMAS
        private long dominioGenericos = 31021000999137;        // VTM generico con componente activo
        private long dominioGenericosConInfo = 31031000999139; // AMPP generico con componente activo, potencia y dosis

        public Form1()
        {
            InitializeComponent();

            // Combobox initialization
            comboBox1.Items.Add(new ComboboxItem("Sustancias", dominioSustancias));
            comboBox1.Items.Add(new ComboboxItem("Problemas", dominioProblemas));
            comboBox1.Items.Add(new ComboboxItem("Generico + Info", dominioGenericosConInfo));


            // carga mapsets corrientes
            mapSet[] ms = ws.obtenerMapSetsCorrientes(subsetIdInstitucion);
            textBox6.Text += "MapSets Corrientes:" + Environment.NewLine;
            foreach (mapSet m in ms)
            {
                textBox6.Text += "id: " + m.id + " desc: " + m.descripcion + Environment.NewLine;
            }


            // ============================================ MEMBERS ============================================= //
            member[] members = ws.obtenerMembers(dominioGenericos, subsetIdInstitucion);
            int max_results = 20;
            int idx = 0;

            textBox6.Text += Environment.NewLine;
            textBox6.Text += "Members:" + Environment.NewLine;
            foreach (member m in members)
            {
                textBox6.AppendText(m.descripcion + "( descId: " + m.descId + ")" + Environment.NewLine);
                idx++;
                if (idx > max_results) break; // just render max_results
            }

            /*
            // Write file with members (test)
            String texto = "";
            foreach (member m in members)
            {
                texto += m.descripcion + "( descId: " + m.descId + ")" + Environment.NewLine;
            }
            System.IO.File.WriteAllText(@".\members.txt", texto);
            */
            // ============================================ MEMBERS ============================================= //


            // la doc habla de un servicio de lista de dominios que no está declarado en el WSDL
            // idem para el servicio de elementos del dominio, esta en la doc, no en el WSDL


            // test de clasificacion de la descripcion del termino dolor de pecho (dorsalgia) en CIE10
            long dolorDePechoDescId = 546041000999119;
            long cie10mapSetId = 101045;
            clasificador[] clasificadores = ws.obtenerClasificador(dolorDePechoDescId, cie10mapSetId, subsetIdInstitucion);


            textBox6.Text += Environment.NewLine;
            textBox6.Text += "test clasificador de 'dolor de pecho' en CIE10:" + Environment.NewLine;
            foreach (clasificador clasificador in clasificadores)
            {
                textBox6.Text += clasificador.id + " " + clasificador.descripcion + Environment.NewLine;
            }


            textBox6.Text += Environment.NewLine;
            textBox6.Text += "Lista de subsets " + Environment.NewLine;
            subset[] sss = ws.obtenerListaDeSubsets(subsetIdInstitucion);
            foreach (subset ss in sss)
            {
                textBox6.Text += ss.subsetid + " " + ss.descripcion + Environment.NewLine;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String txt = textBox1.Text;
            ComboboxItem item = (ComboboxItem)comboBox1.SelectedItem;

            if (item == null)
            {
                MessageBox.Show("Elija un domino de la lista");
                return;
            }

            long subset = (long)item.value;


            ofertaTextoCabecera res = ws.obtenerOfertaTextos(txt, subset, subsetIdInstitucion); // item.value es el id de subset

            //Console.WriteLine( res.ToString() );

            


            if (res != null)
            {
                ofertaTextoDetalle[] detalle = res.ofertaTextoDetalle;

                textBox2.Text = "dominios: " + res.dominios + Environment.NewLine +
                                "entrada: " + res.entrada + Environment.NewLine +
                                "explicacion: " + res.explicacion + Environment.NewLine +
                                "idDescripcion: " + res.idDescripcion + Environment.NewLine +
                                "idDescripcionPreferido: " + res.idDescripcionPreferido + Environment.NewLine +
                                "legible: " + res.legible + Environment.NewLine +
                                "multiplicidad: " + res.multiplicidad + Environment.NewLine +
                                "refinacionObligatoria: " + res.refinacionObligatoria + Environment.NewLine +
                                "textoPreferido: " + res.textoPreferido + Environment.NewLine +
                                "titulo: " + res.titulo + Environment.NewLine;

                if (detalle != null)
                {
                    foreach (ofertaTextoDetalle d in detalle)
                    {
                        // detalle de oferta de textos
                        textBox3.AppendText( d.texto + " (concept id: " + d.conceptId + " / desc. id: " + d.descriptionId + ")" + Environment.NewLine );


                        // atributos de la descripcion
                        descriptionAtributo da = ws.obtenerAtributosDeDescription(d.descriptionId, subsetIdInstitucion);

                        // da.descripcion es lo mismo que d.texto
                        //textBox3.AppendText( " - " + da.descripcion + Environment.NewLine );
                        textBox3.AppendText( " - tipo: " + da.tipo + Environment.NewLine );
                        textBox3.AppendText( " - frecuencia de uso: " + da.frecuenciaUso + Environment.NewLine );
                        //textBox3.AppendText( " - " + da.frecuenciaUsoSpecified + Environment.NewLine );


                        // descripcion data atributo ???
                        // devuelve la misma informacion que ya tengo por ofertaTextoDetalle
                        descripcionDataAtributo dataDesc = ws.obtenerDataDescriptionId(d.descriptionId, subsetIdInstitucion);

                        // termino preferido y descripcion termino tienen los mismos valores
                        textBox3.AppendText(" - data: " + dataDesc.auditoriaTermino +" "+ dataDesc.conceptid +" "+ dataDesc.descripcionTermino + " " + dataDesc.terminoPreferido +" "+ dataDesc.tipoTermino + Environment.NewLine);


                        // subsets de la descripcion
                        // este endpoint no esta documentado, no se para que es el segundo parametro 'flag'
                        subset[] subsets = ws.obtenerSubsetsXDescId(d.descriptionId, "", subsetIdInstitucion);

                        textBox3.AppendText( " - subsets de la descripcion: " + Environment.NewLine );
                        foreach (subset ss in subsets)
                        {
                            textBox3.AppendText( "   - subset id: " + ss.subsetid + " desc: " + ss.descripcion + Environment.NewLine );
                        }
                    }
                }

                
            }
            else
            {
                textBox2.Text = "el resultado fue nulo";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TerminologiaWSClient ws = new TerminologiaWSClient();

            long subsetIdInstitucion = 34701000999132;

            try
            {
                long descId = Convert.ToInt64(textBox5.Text);

                // Segundo pedido
                concept c = ws.obtenerConceptXDescID(descId, subsetIdInstitucion);

                // Muestra la misma info que el resultado de obtenerOfertaTextos en ofertaTextosDetalle
                textBox4.Text += c.concId + Environment.NewLine;
                textBox4.Text += c.descripcion + Environment.NewLine;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Error Title", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // information del medicamento por desc id
        private void button3_Click(object sender, EventArgs e)
        {
            long descId = Convert.ToInt64(textBox7.Text);

            Console.Out.WriteLine("medicamento desc id " + descId.ToString());

            composicionFarmaco[] comp = ws.obtenerComposicionFarmacoXDescId(descId, subsetIdInstitucion);

            if (comp.Length == 0)
            {
                textBox8.AppendText("No se encontraron componentes" + Environment.NewLine);
            }
            else
            {
                foreach (composicionFarmaco c in comp)
                {
                    textBox8.AppendText(c.descripcion + " " + c.descripcionSustancia + " " + c.formaFarmaceutica + " " + c.cantidadSustancia + " " + c.unidadMedida + " " + c.viaAdministracionPreferida + Environment.NewLine);
                }
            }

            member[] mms = ws.obtenerPresentacionesComercialesXGenerico(descId, subsetIdInstitucion);
            textBox8.AppendText("Presentaciones Comerciales X Generico" + Environment.NewLine);
            foreach (member m in mms)
            {
                textBox8.AppendText(m.descripcion + Environment.NewLine);
            }

            
            mms = ws.obtenerGenericoXPresentacionComercial(descId, subsetIdInstitucion);
            textBox8.AppendText("Generico X Presentacion Comercial" + Environment.NewLine);
            foreach (member m in mms)
            {
                textBox8.AppendText(m.descripcion + Environment.NewLine);
            }


            mms = ws.obtenerMedicamentoClinicoPorMedicamentoBasico(descId, subsetIdInstitucion);
            textBox8.AppendText("Medicamento Clinico Por Medicamento Basico" + Environment.NewLine);
            foreach (member m in mms)
            {
                textBox8.AppendText(m.descripcion + Environment.NewLine);
            }


            mms = ws.obtenerMedicamentoBasicoPorMedicamentoClinico(descId, subsetIdInstitucion);
            textBox8.AppendText("Medicamento Basico Por Medicamento Clinico" + Environment.NewLine);
            foreach (member m in mms)
            {
                textBox8.AppendText(m.descripcion + Environment.NewLine);
            }


            composicionGenericoAmbulatorioCabecera cgac = ws.ObtenerComposicionGenericoAmbulatorio(descId, subsetIdInstitucion);
            composicionGenericoAmbulatorioDetalle[] cgad = cgac.genericoAmbulatorioDetalles;
            if (cgad == null)
            {
                textBox8.AppendText("No hay detalles para el generico ambulatorio" + Environment.NewLine);
            }
            else
            {
                foreach (composicionGenericoAmbulatorioDetalle d in cgad)
                {
                    textBox8.AppendText(d.descripcionSustancia + Environment.NewLine);
                }
            }

            //composicionPresentacionAmbulatorioCabecera cpac = ws.ObtenerComposicionPresentacionAmbulatorio(descId, subsetIdInstitucion);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String txt = textBoxGenerico.Text;
            member[] res = ws.obtenerGenericosPorTxtYSubset(txt, dominioGenericosConInfo, subsetIdInstitucion);
            foreach (member m in res)
            {
                textBox10.AppendText(m.descripcion +"( descId: "+ m.descId +")"+ Environment.NewLine);
            }
        }

    }

    public class ComboboxItem
    {
        public string text { get; set; }
        public object value { get; set; }

        public ComboboxItem(string text, object value)
        {
            this.text = text;
            this.value = value;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
