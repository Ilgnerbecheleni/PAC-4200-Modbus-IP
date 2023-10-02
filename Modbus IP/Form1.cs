using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using NModbus;
using NModbus.IO;
using NModbus.Utility;
using NModbus.Device;
using System.Net.Http;
using NModbus.Extensions.Enron;

namespace Modbus_IP
{
    public partial class Form1 : Form
    {
        private IModbusMaster master;
        private TcpClient tcpClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Endereço IP do dispositivo Modbus TCP
                string ipAddress = "192.168.1.20";
                int port = 502;

                // Crie uma instância TcpClient para se conectar ao dispositivo Modbus
                tcpClient = new TcpClient(ipAddress, port);

                // Crie uma instância ModbusIpMaster para a comunicação
                var factory = new ModbusFactory();
                master = factory.CreateIpMaster(new TcpClientAdapter(tcpClient));

                MessageBox.Show("Conexão bem-sucedida!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if(txtIp.Text.Length > 0 && txtInfo.Text.Length > 0 )
                {
                    string ip = txtIp.Text;
                    ushort adress = ushort.Parse(txtInfo.Text);
                    double valor = ReadRegisterValue(ip, adress);
                    txtValor.Text = valor.ToString();
                }
                else
                {
                    MessageBox.Show("Preencha os campos Ip e Adress");
                }
              
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao ler: " + ex.Message);
            }

        }


        public double ReadRegisterValue(string ipAddress, ushort startAddress)
        {
            try
            {
                // Porta do dispositivo Modbus IP (geralmente é 502)
                int port = 502;

                // Quantidade de registros a serem lidos
                ushort numberOfRegisters = 2;

                // Crie uma instância TcpClient para se conectar ao dispositivo Modbus IP
                using (TcpClient tcpClient = new TcpClient(ipAddress, port))
                {
                    // Crie uma instância ModbusIpMaster para a comunicação
                    ModbusFactory factory = new ModbusFactory();
                    IModbusMaster master = factory.CreateMaster(tcpClient);

                    // Faça uma leitura de registrador
                    ushort[] data = master.ReadHoldingRegisters((byte)1, startAddress, numberOfRegisters);

                    // Chame a função de conversão para obter os valores reais
                    double valorReal1 = ConvertToRealValue(data[0], data[1]);

                    return valorReal1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao ler: " + ex.Message);
                return 0.0; // Ou outro valor de retorno apropriado em caso de erro
            }
        }

        public static double ConvertToRealValue(ushort highRegister, ushort lowRegister)
        {
            // Junte os registradores que compõem a grandeza
            int combinedValue = (highRegister << 16) | lowRegister;

            // Extraia o bit de sinal
            bool isNegative = (combinedValue & 0x80000000) != 0;

            // Extraia os 8 bits que indicam a parte inteira
            int intPartBits = (combinedValue >> 23) & 0xFF;

            // Extraia os bits restantes (parte fracionária)
            int fracPartBits = combinedValue & 0x7FFFFF;

            // Calcule o valor real
            double realValue = 0.0;

            if (intPartBits != 0)
            {
                realValue = (isNegative ? -1 : 1) * (1 + (double)fracPartBits / Math.Pow(2, 23)) * Math.Pow(2, intPartBits - 127);
            }

            return realValue;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Feche a conexão com o dispositivo Modbus
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
                master.Dispose();
                master = null;
                MessageBox.Show("Desconectado com sucesso!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (txtIp.Text.Length > 0)
            {
                string ipAddress = txtIp.Text;
                btnStop.Enabled = true;
                btnLer.Enabled = false;
                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Preencha os campos Ip e Adress");
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string ipAddress = txtIp.Text;  // Substitua pelo endereço IP do seu dispositivo

            double tensao = ReadRegisterValue(ipAddress, 1);
            double corrente = ReadRegisterValue(ipAddress, 13);
            double potencia = ReadRegisterValue(ipAddress, 25);
            double frequencia = ReadRegisterValue(ipAddress, 55);



            // Exibir os valores nos TextBoxes
            txtTensao.Text = tensao.ToString("F2") + " V";
            txtCorrente.Text = corrente.ToString("F2") + " A";
            txtPot.Text = potencia.ToString("F2") + " W";
            txtFrequencia.Text = frequencia.ToString("F2") + " Hz";
            
            // Adicione o valor lido como um novo ponto no gráfico
            chart1.Series["Valores"].Points.AddY(tensao);

            // Atualize o gráfico
            chart1.Update();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            btnLer.Enabled = true;
            timer1.Enabled = false;
            timer1.Enabled = false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            txtPot.Text = "60 W";
        }
    }
}
