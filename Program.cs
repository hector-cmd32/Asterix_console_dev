using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Prueba_1_Asterix
{
    public struct dataRecord_struct
    {
        public int cat { get; set; }
        public int longitud { get; set; }
        public byte[] fspec { get; set; }
        public int long_fspec { get; set; }
        public byte[] datos { get; set; }
    }

    public struct time
    {
        public int horas { get; set; }
        public int minutos { get; set; }
        public int segundos { get; set; }
        public int milisegundos { get; set; }
    }

    public struct targetReportDescriptor
    {
        public int TYP { get; set; } // Del 0 al 7
        public Boolean SIM { get; set; }
        public Boolean RDP { get; set; }
        public Boolean SPI { get; set; }
        public Boolean RAB { get; set; }
        public Boolean TST { get; set; }
        public Boolean ERR { get; set; }
        public Boolean XPP { get; set; }
        public Boolean ME { get; set; }
        public Boolean MI { get; set; }
        public int FOE_FRI { get; set; } // Del 0 al 3
        public int ADSB { get; set; } // Del 0 al 3
        public Boolean ADSB_EP { get; set; }
        public Boolean ADSB_VAL { get; set; }
        public int SCN { get; set; } // Del 0 al 3
        public Boolean SCN_EP { get; set; }
        public Boolean SCN_VAL { get; set; }
        public int PAI { get; set; } // Del 0 al 3
        public Boolean PAI_EP { get; set; }
        public Boolean PAI_VAL { get; set; }


        // Quedan más campos, primero lidiar con estos!
    }

    public struct radarPlotCharacteristics
    {
        public double SRL { get; set; } // De 0 a 11.21 grados
        public int SRR { get; set; } // Número de respuestas
        public int SAM { get; set; } // En dBm, puede ser negativo
        public double PRL { get; set; } // De 0 a 11.21 grados
        public int PAM { get; set; } // En dBm, puede ser negativo
        public double RPD { get; set; } // En NM, estudiar las características
        public double APD { get; set; } // En grados
        public int SCO { get; set; }
        public double SCR { get; set; } // En dB
        public double RW { get; set; } // En NM
        public double AR { get; set; } // En NM


    }

    public struct BDSRegisterData
    {
        public string[] data { get; set; }
        public string[] address_1 { get; set; }
        public string[] address_2 { get; set; }
    }

    public struct trackInfo_struct
    {
        public int SAC { get; set; }
        public int SIC { get; set; }
        public time timeOfDay { get; set; }
        public targetReportDescriptor TRD { get; set; }
        public double rho_polar { get; set; } // NM
        public double theta_polar { get; set; } // grados
        public bool mode3A_V { get; set; }
        public bool mode3A_G { get; set; }
        public bool mode3A_L { get; set; }
        public int mode3A_code { get; set; }
        public bool flightLevel_V { get; set; }
        public bool flightLevel_G { get; set; }
        public double flightLevel { get; set; }
        public radarPlotCharacteristics RPC { get; set; }
        public string AC_address { get; set; }
        public string AC_identification { get; set; }
        public BDSRegisterData BDS_rData { get; set; }


    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lectura del archivo en curso...");

            string path;

            path = "C:/Users/hector/source/repos/230502-est-080001_BCN.ast";

            byte[] fileBytes = File.ReadAllBytes(path);
            List<dataRecord_struct> listaDataRecords = new List<dataRecord_struct>();

            int direccionDataRecordProcesando= 0; //Almacenará en todo momento la posición en la que empieza el siguiente Data Record en procesamiento (posición en bytes, que
                                                  //contendrá la Categoría)
            int categoriaActual = -1;
            int numeración = 1;
            int longitudDataRecordProcesando = fileBytes[2]; //En el caso del primer data record estará aquí ¡¡¡Pensar una forma en que me lea los dos bytes del length!!!
            int longitud_fichero = fileBytes.Length;
            Console.WriteLine($"Longitud del fichero: {longitud_fichero}");

            //Extraido del while de debajo para pruebas, debe ir esto: direccionDataRecordProcesando < fileBytes.Length
            while (numeración < 5) //Nos permite crear una lista con todos los data records de la categoría 48
            {
                longitudDataRecordProcesando = fileBytes[direccionDataRecordProcesando + 2];
                Console.WriteLine($"Data record número: {numeración}");
                //Console.WriteLine($"Longitud del data record: {longitudDataRecordProcesando}");
                //Antes de añadir el data record nos cercioraremos que pertenezca a la categoría 48
                categoriaActual = fileBytes[direccionDataRecordProcesando];
                if (categoriaActual == 48)
                {
                    //Console.WriteLine($"Aceptado: data record de categoría: {categoriaActual}");

                    //Recuperamos todos los bytes del data record
                    byte[] dataRecord = new byte[longitudDataRecordProcesando];
                    int j;
                    for (j = 6; j < dataRecord.Length; j++)
                    {
                        dataRecord[j-6] = fileBytes[direccionDataRecordProcesando+j];
                    }

                    //Primero de todo averiguamos la longitud del FSPEC


                    //Recuperamos todos los bytes del FSPEC
                    j = 0;
                    int longitudFSPEC = 1;
                    int punteroFSPEC = direccionDataRecordProcesando + 3;
                    byte byteFSPEC_evaluado; 
                    bool sigue = true;
                    while (sigue)
                    {
                        byteFSPEC_evaluado = fileBytes[punteroFSPEC]; //FSPEC que estamos evaluando para saber si hay extensión
                        bool septimoBit = (byteFSPEC_evaluado & (1 << 7)) != 0;

                        if (septimoBit)
                        {
                            //Console.WriteLine("Bit de extensión activo, continuamos");
                            longitudFSPEC++;
                            punteroFSPEC ++;
                        }
                        else
                        {
                            //Console.WriteLine($"Bit de extensión inactivo, FSPEC con longitud total de {longitudFSPEC} byte/s");
                            sigue = false;
                        }
                    }

                    //Recuperamos todos los bytes del FSPEC
                    byte[] fspecActual = new byte[longitudFSPEC];
                    for (j = 0; j < fspecActual.Length; j++)
                    {
                        fspecActual[j] = fileBytes[direccionDataRecordProcesando + 3 + j];
                    }

                    dataRecord_struct item = new dataRecord_struct();

                    item.cat = categoriaActual;
                    item.longitud = longitudDataRecordProcesando;
                    item.datos = dataRecord;
                    item.fspec = fspecActual;
                    item.long_fspec = longitudFSPEC;

                    listaDataRecords.Add(item);
                    
                }
                else //Este else irá fuera por términos de velocidad
                {
                    Console.WriteLine($"Denegado: data record de categoría: {categoriaActual}");
                    Console.WriteLine($"Saltando al siguiente data record...");
                }
                //Console.WriteLine($"Tamaño de lista de dataRecords: {listaDataRecords.Count}"); línea para controlar cómo se llena la lista
                direccionDataRecordProcesando = direccionDataRecordProcesando + longitudDataRecordProcesando;
                numeración++;
                //Thread.Sleep(250);
            }

            Console.WriteLine($"Tamaño de lista de dataRecords: {listaDataRecords.Count}");

            List<trackInfo_struct> listaTracks = new List<trackInfo_struct>();

            int i = 0;
            dataRecord_struct dataRecordProcesando;
            while (i< listaDataRecords.Count) //Recorremos la lista de data records y los almacenamos en su estructura
            {
                // Nos creamos un objeto de tipo trackInfo y los vamos rellenando:
                trackInfo_struct track = new trackInfo_struct();

                dataRecordProcesando = listaDataRecords[i];

                //Nos crearemos un vector con todos los datafields presentes en el datarecord:
                List<int> dataFields_presentes = new List<int>();
                int j = 0;
                while (j < dataRecordProcesando.long_fspec * 7)
                {
                    decimal d = j / 7;
                    int byteInteres = Decimal.ToInt32(d);

                    int bitInteres = j - 7 * byteInteres;
                    bool siDataField = (dataRecordProcesando.fspec[byteInteres] & (1 << (7-bitInteres))) != 0;
                    if (siDataField)
                    {
                        //Si está a 1 el bit del datafield correspondiente, procedemos a coger la información:
                        dataFields_presentes.Add(j+1);
                        Console.WriteLine(j + 1);

                    }
                    j++;
                }

                //Una vez listados los data fields presentes, nos recorremos la lista para ir rellenando la lista de tracks:
                int puntoProcesado = 0; //variable que nos permite saber en que lugar del campo de datos estamos, para seguir cogiendo campos
                byte[] datosProcesando = dataRecordProcesando.datos;

                byte byteMSB;
                byte byteMedio;
                byte byteLSB;

                int REP;
                int h; // Variable para bucle while dentro de los casos
                int f; // Variable para bucle for dentro de los casos

                byte mascara;
                byte bitsDeInteres;

                foreach (int numero in dataFields_presentes)
                {
                    switch (numero)
                    {
                        //Data Source Identifier
                        case 1: 
                            Console.WriteLine("Caso 1");
                            track.SAC = datosProcesando[puntoProcesado];
                            track.SIC = datosProcesando[puntoProcesado + 1];

                            puntoProcesado += 2;

                            break;
                        //Time-OF-Day
                        case 2: 
                            Console.WriteLine("Caso 2");

                            byteMSB = datosProcesando[puntoProcesado];
                            byteMedio = datosProcesando[puntoProcesado+1];
                            byteLSB = datosProcesando[puntoProcesado+2];

                            // Combinamos los bytes en un valor entero
                            double tiempoTotal = (byteMSB << 16) | (byteMedio << 8) | byteLSB;

                            double totalSegundos = (tiempoTotal / 128);

                            // Calculamos las horas, minutos y segundos
                            int horas = (int)(totalSegundos / 3600);
                            int minutos = (int)((totalSegundos % 3600) / 60);
                            int segundos = (int)(totalSegundos % 60);
                            int milisegundos = (int)((totalSegundos * 1000) % 1000);

                            Console.WriteLine($"Hora actual: {horas:D2}:{minutos:D2}:{segundos:D2}.{milisegundos:D3}");

                            time t = new time();

                            t.horas = horas;
                            t.minutos = minutos;
                            t.segundos = segundos;
                            t.milisegundos = milisegundos;

                            track.timeOfDay = t;

                            puntoProcesado += 3;

                            break;
                        // Target Report Descriptor
                        case 3: 
                            Console.WriteLine("Caso 3");

                            // Primero nos buscamos la longitud total que tendrá el data field:
                            // Comprobamos si el campo FX está activo, si es el caso, deberemos mirar los siguientes:
                            bool sigue = true;
                            int longitudDatafield = 1;
                            int z = 0;
                            while (sigue)
                            {
                                bool septimoBit = (datosProcesando[puntoProcesado+z] & (1 << 0)) != 0;
                                if (septimoBit)
                                {
                                    //Console.WriteLine("Bit de extensión activo, continuamos");
                                    longitudDatafield++;
                                    z++;
                                }
                                else
                                {
                                    sigue = false;
                                }
                            }

                            // Creamos una estructura de tipo target report descriptor y empezamos a rellenarla:
                            targetReportDescriptor trd = new targetReportDescriptor();

                            // TYP: son los 3 primeros bits (empezando por el MSb), los cuales guardamos en decimal.
                            // Nos creamos una máscara para recuperar los 3 primeros bits del byte 1 (TYP)

                            mascara = 0b11100000;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara

                            // Desplazamos los bits de interés a la posición 0
                            int typ = (byte)(bitsDeInteres >> 5);

                            trd.TYP = typ;

                            // SIM: es el bit 4 (empezando por el LSb, que es lo que asumiremos si no se indica lo contrario
                            trd.SIM = (datosProcesando[puntoProcesado] & (1 << 4)) != 0;

                            // RDP: es el bit 3
                            trd.RDP = (datosProcesando[puntoProcesado] & (1 << 3)) != 0;

                            // SPI: es el bit 2
                            trd.SPI = (datosProcesando[puntoProcesado] & (1 << 2)) != 0;

                            // RAB: es el bit 1
                            trd.RAB = (datosProcesando[puntoProcesado] & (1 << 1)) != 0;

                            if(longitudDatafield == 1) // Continuamos sólo si el datafield es mayor a 1 byte
                            { 
                                track.TRD = trd; // Finalmente añadimos al campo TRD el objeto trd con todos los campos
                                puntoProcesado += 1;
                                break;
                            }

                            // Procedemos con el primer byte de extensión

                            // TST: es el bit 7
                            trd.TST = (datosProcesando[puntoProcesado + 1] & (1 << 7)) != 0;

                            // ERR: es el bit 6
                            trd.ERR = (datosProcesando[puntoProcesado + 1] & (1 << 6)) != 0;

                            // XPP: es el bit 5
                            trd.XPP = (datosProcesando[puntoProcesado + 1] & (1 << 5)) != 0;

                            // ME: es el bit 4
                            trd.ME = (datosProcesando[puntoProcesado + 1] & (1 << 4)) != 0;

                            // MI: es el bit 3
                            trd.MI = (datosProcesando[puntoProcesado + 1] & (1 << 3)) != 0;

                            // TYP: son los 3 primeros bits (empezando por el MSb), los cuales guardamos en decimal.
                            // Nos creamos una máscara para recuperar los 3 primeros bits del byte 1 (TYP)

                            mascara = 0b00000110;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado + 1] & mascara); // Aplicamos la máscara

                            // Desplazamos los bits de interés a la posición 0
                            int foe_fri = (byte)(bitsDeInteres >> 5);

                            trd.FOE_FRI = foe_fri;

                            if (longitudDatafield == 2) // Continuamos sólo si el datafield es mayor a 2 bytes
                            { 
                                track.TRD = trd; // Finalmente añadimos al campo TRD el objeto trd con todos los campos
                                puntoProcesado += 2;
                                break;
                            }

                            // Procedemos con el segundo byte de extensión

                            // ADSB_EP: es el bit 7
                            trd.ADSB_EP = (datosProcesando[puntoProcesado + 2] & (1 << 7)) != 0;

                            // ADSB_VAL: es el bit 6
                            trd.ADSB_VAL = (datosProcesando[puntoProcesado + 2] & (1 << 6)) != 0;

                            // SCN_EP: es el bit 5
                            trd.SCN_EP = (datosProcesando[puntoProcesado + 2] & (1 << 5)) != 0;

                            // SCN_VAL: es el bit 4
                            trd.SCN_VAL = (datosProcesando[puntoProcesado + 2] & (1 << 4)) != 0;

                            // PAI_EP: es el bit 3
                            trd.PAI_EP = (datosProcesando[puntoProcesado + 2] & (1 << 3)) != 0;

                            // PAI_VAL: es el bit 2
                            trd.PAI_VAL = (datosProcesando[puntoProcesado + 2] & (1 << 2)) != 0;

                            // Como ya no podemos tener más bytes de extensión, procedemos a añadir el objeto TRD y salimos del caso
                            track.TRD = trd;
                            puntoProcesado += 3;
                            break;
                        // Measured position in Polar Coordinates
                        case 4:
                            Console.WriteLine("Caso 4");
                            byte rho_byteMSB = datosProcesando[puntoProcesado];
                            byte rho_byteLSB = datosProcesando[puntoProcesado + 1];

                            byte theta_byteMSB = datosProcesando[puntoProcesado + 2];
                            byte theta_byteLSB = datosProcesando[puntoProcesado + 3];

                            // Combinamos los bytes en un valor entero
                            double rho = (rho_byteMSB << 8) | rho_byteLSB;
                            double theta = (theta_byteMSB << 8) | theta_byteLSB;

                            double rho_NM = (rho/256);
                            double theta_grados = (theta*360)/Math.Pow(2, 16);

                            track.rho_polar = rho_NM;
                            track.theta_polar = theta_grados;

                            puntoProcesado += 4;

                            break;
                        // Mode-3/A Code in Octal Representation
                        case 5:
                            Console.WriteLine("Caso 5");
                            // Nos empezamos por sacar el código, dígito a dígito, con ayuda de máscaras que cogen bits de 4 en 4

                            // Primer dígito, 1r byte, bits del 1 al 3:
                            mascara = 0b00001110;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara
                            // Desplazamos los bits de interés a la posición 0
                            int digito_1 = (byte)(bitsDeInteres >> 1);

                            // Segundo dígito, 1r y segundo byte, bit 0 (1r byte) y 6,7 (2o byte):
                            mascara = 0b00000001;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara
                            byte mascara2 = 0b11000000;
                            byte bitsDeInteres2 = (byte)(datosProcesando[puntoProcesado + 1] & mascara2);
                            int digito_2 = (byte)((bitsDeInteres << 3) | (bitsDeInteres2 >> 6));

                            // Tercero dígito, 2o byte, bits del 3 al 5:
                            mascara = 0b00111000;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado + 1] & mascara); // Aplicamos la máscara
                            // Desplazamos los bits de interés a la posición 0
                            int digito_3 = (byte)(bitsDeInteres >> 3);

                            // Cuarto dígito, 2o byte, bits del 0 al 2:
                            mascara = 0b00000111;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado + 1] & mascara); // Aplicamos la máscara
                            // Desplazamos los bits de interés a la posición 0
                            int digito_4 = (byte)(bitsDeInteres);

                            track.mode3A_code = digito_1 * 1000 + digito_2 * 100 + digito_3 * 10 + digito_4;

                            track.mode3A_V = (datosProcesando[puntoProcesado] & (1 << 7)) != 0;
                            track.mode3A_G = (datosProcesando[puntoProcesado] & (1 << 6)) != 0;
                            track.mode3A_L = (datosProcesando[puntoProcesado] & (1 << 5)) != 0;

                            puntoProcesado += 2;

                            break;
                        // Flight Level in Binary Representation
                        case 6:
                            Console.WriteLine("Caso 6");
                            // En primer lugar, nos sacamos el flight level:
                            // Primera parte, 1r byte, bits del 0 al 5:
                            mascara = 0b00011111;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara

                            double total = (bitsDeInteres << 8) | (datosProcesando[puntoProcesado + 1]);
                            double flightLevel_FL = total / 4;

                            track.flightLevel = flightLevel_FL;

                            track.flightLevel_V = (datosProcesando[puntoProcesado] & (1 << 7)) != 0;
                            track.flightLevel_G = (datosProcesando[puntoProcesado] & (1 << 6)) != 0;

                            puntoProcesado += 2;

                            break;
                        // Radar Plot Characteristics :/
                        case 7:
                            Console.WriteLine("Caso 7");
                            // Lo primero será identificar que subcampos existen en nuestro datafield:
                            // Nos crearemos un vector con todos los datafields presentes en el datarecord:
                            List<int> dataSubfields_presentes = new List<int>();

                            // Ahora nos creamos un objeto de tipo Radar Plot Char. para almacenar los datos:
                            radarPlotCharacteristics rpc = new radarPlotCharacteristics();

                            h = 0;
                            while (h < 7)
                            {
                                bool siDataSubield = (datosProcesando[puntoProcesado] & (1 << (7 - h))) != 0;
                                if (siDataSubield)
                                {
                                    //Si está a 1 el bit del dataSubfield correspondiente, procedemos a coger la información:
                                    dataSubfields_presentes.Add(h + 1);
                                }
                                h++;
                            }

                            puntoProcesado += 1;

                            // Comrpobamos si está activo el bit de extensión (FX):
                            bool siDataSubield_FX = (datosProcesando[puntoProcesado] & (1 << (0))) != 0;
                            if (siDataSubield_FX)
                            {
                                //Si es así, añadiremos los campos de extensión como 11, 12, aprovechando así la lista existente:
                                h = 0;
                                while (h < 7)
                                {
                                    bool siDataSubield = (datosProcesando[puntoProcesado] & (1 << (7 - h))) != 0;
                                    if (siDataSubield)
                                    {
                                        //Si está a 1 el bit del dataSubfield correspondiente, procedemos a coger la información:
                                        dataSubfields_presentes.Add(10 + h + 1);
                                    }
                                    h++;
                                }
                                puntoProcesado += 1;
                            }

                            // Una vez identificados todos los subfields presentes, recorremos la lista y recuperamos la información:
                            foreach (int subField in dataSubfields_presentes)
                            {
                                bool negativo;
                                switch (subField)
                                {
                                    case 1:
                                        rpc.SRL = (double)(datosProcesando[puntoProcesado] * (360 / Math.Pow(2, 13)));
                                        puntoProcesado += 1;
                                        break;

                                    case 2:
                                        rpc.SRR = datosProcesando[puntoProcesado];
                                        puntoProcesado += 1;
                                        break;
                                    case 3:
                                        // Primero comprobamos si es negativo
                                        negativo = (datosProcesando[puntoProcesado] & (1 << (7))) != 0;
                                        if (negativo)
                                        {
                                            rpc.SAM = convertirDeComplementoA2(datosProcesando[puntoProcesado]);
                                        }
                                        else // Si no es negativo, directamente guardamos su valor:
                                        {
                                            rpc.SAM = datosProcesando[puntoProcesado];
                                        }
                                        puntoProcesado += 1;
                                        break;

                                    case 4:
                                        rpc.PRL = (double)(datosProcesando[puntoProcesado] * (360 / Math.Pow(2, 13)));
                                        puntoProcesado += 1;
                                        break;

                                    case 5:
                                        // Primero comprobamos si es negativo
                                        negativo = (datosProcesando[puntoProcesado] & (1 << (7))) != 0;
                                        if (negativo)
                                        {
                                            rpc.PAM = convertirDeComplementoA2(datosProcesando[puntoProcesado]);
                                        }
                                        else
                                        {
                                            rpc.PAM = datosProcesando[puntoProcesado];
                                        }
                                        puntoProcesado += 1;
                                        break;

                                    case 6:
                                        // Primero comprobamos si es negativo
                                        negativo = (datosProcesando[puntoProcesado] & (1 << (7))) != 0;
                                        if (negativo)
                                        {
                                            rpc.RPD = (double)(convertirDeComplementoA2(datosProcesando[puntoProcesado]))/256;
                                        }
                                        else
                                        {
                                            rpc.RPD = (double)(datosProcesando[puntoProcesado])/256;
                                        }
                                        puntoProcesado += 1;
                                        break;

                                    case 7:
                                        // Primero comprobamos si es negativo
                                        negativo = (datosProcesando[puntoProcesado] & (1 << (7))) != 0;
                                        if (negativo)
                                        {
                                            rpc.APD = (double)convertirDeComplementoA2(datosProcesando[puntoProcesado]) * (360 / Math.Pow(2, 14));
                                        }
                                        else
                                        {
                                            rpc.APD = (double)(datosProcesando[puntoProcesado] * (360 / Math.Pow(2, 14)));
                                        }
                                        puntoProcesado += 1;
                                        break;

                                    case 11:
                                        rpc.SCO = datosProcesando[puntoProcesado];
                                        puntoProcesado += 1;
                                        break;

                                    case 12:
                                        rpc.SCR = ((datosProcesando[puntoProcesado] << 8) | datosProcesando[puntoProcesado + 1])*0.1;
                                        puntoProcesado += 2;
                                        break;

                                    case 13:
                                        rpc.RW = ((datosProcesando[puntoProcesado] << 8) | datosProcesando[puntoProcesado + 1]) * (1/256);
                                        puntoProcesado += 2;
                                        break;

                                    case 14:
                                        rpc.AR = ((datosProcesando[puntoProcesado] << 8) | datosProcesando[puntoProcesado + 1]) * (1 / 256);
                                        puntoProcesado += 2;
                                        break;
                                }
                            }

                            track.RPC = rpc;

                            break;
                        // Aircraft Address
                        case 8:
                            Console.WriteLine("Caso 8");
                            // Unimos toda la información en un mismo entero para ser procesada:
                            int decimalValue = (datosProcesando[puntoProcesado] << 16) | (datosProcesando[puntoProcesado + 1] << 8) | (datosProcesando[puntoProcesado + 2]);
                            string hexValue = decimalValue.ToString("X");

                            // Pasamos el valor hexadecimal al campo correspondiente:
                            track.AC_address = hexValue;

                            puntoProcesado += 3;

                            break;
                        // Aircraft Identification
                        case 9:
                            Console.WriteLine("Caso 9");
                            // Iremos seleccionando de 6 bits en 6 bits para hallar el carácter ASCII correspondiente:
                            int char1 = (datosProcesando[puntoProcesado] & 0b11111100 ) >> 2;
                            int grupo1 = (datosProcesando[puntoProcesado] << 16) | (datosProcesando[puntoProcesado + 1] << 8) | (datosProcesando[puntoProcesado + 2]);
                            int grupo2 = (datosProcesando[puntoProcesado + 3] << 16) | (datosProcesando[puntoProcesado + 4] << 8) | (datosProcesando[puntoProcesado + 5]);

                            // Creamo la máscara y luego la aplicamos al grupo correspondiente para ir obteniendo los carácteres ASCII
                            //mascara = 0b111111000000000000;

                            puntoProcesado += 6;

                            break;
                        // BDS Register Data
                        case 10:
                            Console.WriteLine("Caso 10");
                            // Primero nos buscamos el Repetition Factor:
                            REP = datosProcesando[puntoProcesado];

                            // Nos creamos un vector de strings con los items que indique el REP:
                            string[] listaData = new string[REP];
                            string[] listaAddress_1 = new string[REP];
                            string[] listaAddress_2 = new string[REP];

                            BDSRegisterData objetoBDS = new BDSRegisterData();

                            puntoProcesado += 1;

                            h = 0;

                            string dataIntermedio;
                            string dataFinal;

                            while (h < REP)
                            {
                                dataFinal = "";
                                for(f = 0; f < 7; f++)
                                {
                                    dataIntermedio = datosProcesando[puntoProcesado + f].ToString("X");
                                    dataFinal = dataFinal + dataIntermedio;
                                }
                                listaData[h] = dataFinal;

                                puntoProcesado += 7;

                                listaAddress_1[h] = datosProcesando[puntoProcesado].ToString("X");
                                listaAddress_2[h] = datosProcesando[puntoProcesado + 1].ToString("X");

                                puntoProcesado += 2;
                                h++;
                            }

                            objetoBDS.data = listaData;
                            objetoBDS.address_1 = listaAddress_1;
                            objetoBDS.address_2 = listaAddress_2;

                            track.BDS_rData = objetoBDS;


                            break;
                        //Track Number
                        case 11:
                            Console.WriteLine("Caso 11");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 12:
                            Console.WriteLine("Caso 12");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 13:
                            Console.WriteLine("Caso 13");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 14:
                            Console.WriteLine("Caso 14");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 15:
                            Console.WriteLine("Caso 15");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 16:
                            Console.WriteLine("Caso 16");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 17:
                            Console.WriteLine("Caso 17");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 18:
                            Console.WriteLine("Caso 18");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 19:
                            Console.WriteLine("Caso 19");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 20:
                            Console.WriteLine("Caso 20");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 21:
                            Console.WriteLine("Caso 21");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 22:
                            Console.WriteLine("Caso 22");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 23:
                            Console.WriteLine("Caso 23");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 24:
                            Console.WriteLine("Caso 24");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 25:
                            Console.WriteLine("Caso 25");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 26:
                            Console.WriteLine("Caso 26");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 27:
                            Console.WriteLine("Caso 27");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        case 28:
                            Console.WriteLine("Caso 28");

                            puntoProcesado += 2; // Cambiar según toque

                            break;
                        default:
                            Console.WriteLine("Data field fuera de rango");
                            break;
                    }
                }
                i++;
            }
        }

        static int convertirDeComplementoA2(byte byteComplementoA2)
        {
            int valor_covnertido = 1; // Si la función acaba devolviendo 1, hay un error puesto que debe ser negativo; 

            // Primero invertimos todos los bits y le sumamos 1:
            byte intermedio = (byte)~byteComplementoA2;
            intermedio += 1;

            // Finalmente le aplicamos una máscara para deshacernos del primer bit que indica que es negativo:
            byte mascara = 0b01111111;
            intermedio = (byte)(intermedio & mascara);
            valor_covnertido = (-1) * intermedio;

            return valor_covnertido;
        }
    }
}
