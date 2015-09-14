using System;
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
        private long subsetIdInstitucion = 34701000999132;
        private long dominioProblemas = 601000999132; // se podria tener una lista de dominios

        public Form1()
        {
            InitializeComponent();

            // carga mapsets corrientes
            mapSet[] ms = ws.obtenerMapSetsCorrientes(subsetIdInstitucion);
            foreach (mapSet m in ms)
            {
                textBox6.Text += "id: " + m.id + " desc: " + m.descripcion + Environment.NewLine;
            }

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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String txt = textBox1.Text;
            ofertaTextoCabecera res = ws.obtenerOfertaTextos(txt, dominioProblemas, subsetIdInstitucion);

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
    }
}
