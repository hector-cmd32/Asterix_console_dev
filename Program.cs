using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiCAT6.Utils;

namespace Prueba_1_Asterix
{
    public struct dataRecord_struct
    {
        public int cat { get; set; }
        public int longitud { get; set; }
        public byte[] fspec { get; set; }
        public int long_fspec { get; set; }
        public byte[] datos { get; set; }
        public List<int> dataFields { get; set; }
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
        // BDS 4,0
        public string MCP_FCU_selectedAltitude { get; set; }
        public string FMS_selectedAltitude { get; set; }
        public string barometricPressureSetting { get; set; }
        public string VNAV_mode { get; set; }
        public string altHold_mode { get; set; }
        public string approach_mode { get; set; }
        public string targetAltSource { get; set; }

        // BDS 5,0
        public string rollAngle { get; set; }
        public string trueTrackAngle { get; set; }
        public string groundSpeed { get; set; }
        public string trackAngleRate { get; set; }
        public string trueAirspeed { get; set; }

        // BDS 6,0
        public string magnetigHeading { get; set; }
        public string indicatedAirspeed { get; set; }
        public string MACH { get; set; }
        public string barometricAltitudeRate { get; set; }
        public string intertialVerticalVelocity { get; set; }

    }
    public struct cartesianCoordinates
    {
        public double x { get; set; }
        public double y { get; set; }
    }
    public struct geodesicCoordinates
    {
        public double latitude { set; get; }
        public double longitude { set; get; }
        public double height { set; get; }
    }
    public struct trackStatus
    {
        public Boolean CNF { get; set; }
        public int RAD { get; set; }
        public Boolean DOU { get; set; }
        public Boolean MAH { get; set; }
        public int CDM { get; set; }
        public Boolean TRE { get; set; }
        public Boolean GHO { get; set; }
        public Boolean SUP { get; set; }
        public Boolean TCC { get; set; }
    }
    public struct acasStatus
    {
        public int COM { get; set; }
        public int STAT { get; set; }
        public Boolean SI { get; set; }
        public Boolean MSSC { get; set; }
        public Boolean ARC { get; set; }
        public Boolean AIC { get; set; }
        public Boolean B1A { get; set; }
        public int B1B { get; set; }
    }
    public struct trackInfo_struct
    {
        public string SAC { get; set; }
        public string SIC { get; set; }
        public time timeOfDay { get; set; }
        public targetReportDescriptor TRD { get; set; }
        public double rho_polar { get; set; } // NM
        public double theta_polar { get; set; } // grados
        public geodesicCoordinates coordenadasGeodesicas { get; set; }
        public bool mode3A_V { get; set; }
        public bool mode3A_G { get; set; }
        public bool mode3A_L { get; set; }
        public int mode3A_code { get; set; }
        public bool flightLevel_V { get; set; }
        public bool flightLevel_G { get; set; }
        public double flightLevel { get; set; }
        public double realAltitude { get; set; }
        public radarPlotCharacteristics RPC { get; set; }
        public string AC_address { get; set; }
        public string AC_identification { get; set; }
        public BDSRegisterData BDS_rData { get; set; }
        public int trackNumber { get; set; }
        public cartesianCoordinates cartesianCoord { get; set; }
        public double calc_groundspeed { get; set; } // NM/s
        public double calc_heading { get; set; } // Grados
        public trackStatus status { get; set; }
        public int height3D { get; set; }
        public acasStatus a_status { get; set; }
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
            //Console.WriteLine($"Longitud del fichero: {longitud_fichero}");

            //Extraido del while de debajo para pruebas, debe ir esto: direccionDataRecordProcesando < fileBytes.Length
            while (direccionDataRecordProcesando < fileBytes.Length) //Nos permite crear una lista con todos los data records de la categoría 48
            {
                longitudDataRecordProcesando = fileBytes[direccionDataRecordProcesando + 2];
                //Console.WriteLine($"Longitud del data record: {longitudDataRecordProcesando}");
                //Antes de añadir el data record nos cercioraremos que pertenezca a la categoría 48
                categoriaActual = fileBytes[direccionDataRecordProcesando];
                if (categoriaActual == 48)
                {
                    //Console.WriteLine($"Aceptado: data record de categoría: {categoriaActual}");


                    //Primero de todo averiguamos la longitud del FSPEC:

                    int j = 0;
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
                            punteroFSPEC++;
                        }
                        else
                        {
                            //Console.WriteLine($"Bit de extensión inactivo, FSPEC con longitud total de {longitudFSPEC} byte/s");
                            sigue = false;
                        }
                    }

                    //Recuperamos todos los bytes del data record
                    byte[] dataRecord = new byte[longitudDataRecordProcesando-3];
                    for (j = 3 + longitudFSPEC; j < dataRecord.Length; j++)
                    {
                        dataRecord[j - (3 + longitudFSPEC)] = fileBytes[direccionDataRecordProcesando+j];
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

            //Console.WriteLine($"Tamaño de lista de dataRecords: {listaDataRecords.Count}");

            List<trackInfo_struct> listaTracks = new List<trackInfo_struct>();

            int i = 0;
            dataRecord_struct dataRecordProcesando;
            while (i< listaDataRecords.Count) //Recorremos la lista de data records y los almacenamos en su estructura
            {
                //Console.WriteLine($"     ****************************************     ");
                //Console.WriteLine($"              **********************              ");
                //Console.WriteLine($"     ****************************************     ");
                //Console.WriteLine($"Printeando información del datarecord: {i + 1}...");

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
                    }
                    j++;
                }

                dataRecordProcesando.dataFields = dataFields_presentes;

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

                            track.SAC = datosProcesando[puntoProcesado].ToString("X");
                            track.SIC = datosProcesando[puntoProcesado + 1].ToString("X");

                            //Console.WriteLine($"SAC: {track.SAC}, SIC: {track.SIC}");

                            puntoProcesado += 2;

                            break;
                        //Time-OF-Day
                        case 2: 

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

                            //Console.WriteLine($"Hora del data record: {horas:D2}:{minutos:D2}:{segundos:D2}.{milisegundos:D3}");

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

                            //Console.WriteLine($"Coordenada rho: {rho_NM}NM, coordenada theta: {theta_grados}º");

                            break;
                        // Mode-3/A Code in Octal Representation
                        case 5:

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

                            //Console.WriteLine($"Código del modo3A: {track.mode3A_code}");

                            break;
                        // Flight Level in Binary Representation
                        case 6:

                            // En primer lugar, nos sacamos el flight level:
                            // Primera parte, 1r byte, bits del 0 al 5:
                            mascara = 0b00011111;
                            bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara

                            int total = (bitsDeInteres << 8) | (datosProcesando[puntoProcesado + 1]);
                            double flightLevel_FL = total / 4;

                            /*if (i == 1603)
                            {
                                Console.WriteLine("Hola");
                            }*/

                            if ((datosProcesando[puntoProcesado] & 0b00100000) != 0)
                            {
                                track.flightLevel = convertirDeComplementoA2_short(total);
                            }
                            else
                            {
                                track.flightLevel = flightLevel_FL;
                            }
                            

                            track.flightLevel_V = (datosProcesando[puntoProcesado] & (1 << 7)) != 0;
                            track.flightLevel_G = (datosProcesando[puntoProcesado] & (1 << 6)) != 0;

                            puntoProcesado += 2;

                            //Console.WriteLine($"Flight level: {flightLevel_FL}");

                            break;
                        // Radar Plot Characteristics :/
                        case 7:

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
                            bool siDataSubield_FX = (datosProcesando[puntoProcesado - 1] & (1 << (0))) != 0;
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
                                        //Console.WriteLine($"SSR Plot Runlength (SRL): {rpc.SRL}º");
                                        break;

                                    case 2:
                                        rpc.SRR = datosProcesando[puntoProcesado];
                                        puntoProcesado += 1;
                                        //Console.WriteLine($"Respuestas recibidas para (M)SSR (SRR): {rpc.SRR}");
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
                                        //Console.WriteLine($"Amplitud de la respuesta (M)SSR (SAM): {rpc.SAM}dBm");
                                        break;

                                    case 4:
                                        rpc.PRL = (double)(datosProcesando[puntoProcesado] * (360 / Math.Pow(2, 13)));
                                        puntoProcesado += 1;
                                        //Console.WriteLine($"Primary Plot Runlength (PRL): {rpc.PRL}º");
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
                                        //Console.WriteLine($"Amplitud del Primary Plot (PAM): {rpc.PAM}dBm");
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
                                        //Console.WriteLine($"Diferencia en rango entre PSR y SSR (RPD): {rpc.RPD}NM");
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
                                        //Console.WriteLine($"Diferencia en azimut entre PSR y SSR (APD): {rpc.APD}º");
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

                            // Unimos toda la información en un mismo entero para ser procesada:
                            int decimalValue = (datosProcesando[puntoProcesado] << 16) | (datosProcesando[puntoProcesado + 1] << 8) | (datosProcesando[puntoProcesado + 2]);
                            string hexValue = decimalValue.ToString("X");

                            // Pasamos el valor hexadecimal al campo correspondiente:
                            track.AC_address = hexValue;

                            puntoProcesado += 3;
                            //Console.WriteLine($"Aircraft Address: {hexValue}");

                            break;
                        // Aircraft Identification
                        case 9:


                            string AC_identification = "";

                            // Primero nos creamos una lista de carácteres:
                            List<byte> chars = new List<byte>
                            {
                                (byte)((datosProcesando[puntoProcesado] & 0b11111100) >> 2),
                                (byte)(((datosProcesando[puntoProcesado] & 0b00000011) << 4) | (datosProcesando[puntoProcesado + 1] & 0b11110000) >> 4),
                                (byte)((datosProcesando[puntoProcesado + 1] & 0b00001111) << 2 | (datosProcesando[puntoProcesado + 2] & 0b11000000) >> 6),
                                (byte)(datosProcesando[puntoProcesado + 2] & 0b00111111),
                                (byte)((datosProcesando[puntoProcesado + 3] & 0b11111100) >> 2),
                                (byte)((datosProcesando[puntoProcesado + 3] & 0b00000011) << 4 | (datosProcesando[puntoProcesado + 4] & 0b11110000) >> 4),
                                (byte)((datosProcesando[puntoProcesado + 4] & 0b00001111) << 2 | (datosProcesando[puntoProcesado + 5] & 0b11000000) >> 6),
                                (byte)(datosProcesando[puntoProcesado + 5] & 0b00111111)
                            };

                            // Ahora recorremos la lista y vamos convirtiendo los 6 bits a carácteres:
                            foreach (byte character in chars)
                            {
                                bool bit1, bit2, bit3, bit4, bit5, bit6;

                                // Siguiendo la tabla proporcionada por el profesor, iremos leyendo bits y acotando la zona de búsqueda del
                                // carácter necesario. Primero nos leemos todos los bits para luego trabajar con ellos:
                                bit1 = (character & 0b00000001) != 0;
                                bit2 = (character & 0b00000010) != 0;
                                bit3 = (character & 0b00000100) != 0;
                                bit4 = (character & 0b00001000) != 0;
                                bit5 = (character & 0b00010000) != 0;
                                bit6 = (character & 0b00100000) != 0;

                                if(bit6 == true && bit5 == false)
                                {
                                    AC_identification += " ";
                                }
                                else if(bit6 == true && bit5 == true)
                                {
                                    AC_identification += (character & 0b00001111);
                                }
                                else if (bit6 == false && bit5 == false)
                                {
                                    char letra = (char)('A' + (character & 0b00001111) - 1);
                                    AC_identification += letra.ToString();
                                }
                                else if (bit6 == false && bit5 == true)
                                {
                                    char letra = (char)('P' + (character & 0b00001111));
                                    AC_identification += letra.ToString();
                                }
                            }
                            track.AC_identification = AC_identification;
                            
                            puntoProcesado += 6;

                            //Console.WriteLine($"Aircraft Identifiaction: {AC_identification}");

                            break;
                        // BDS Register Data
                        case 10:

                            // Primero nos buscamos el Repetition Factor:
                            REP = datosProcesando[puntoProcesado];

                            byte byteBDS;
                            int BDS1;
                            int BDS2;

                            BDSRegisterData objetoBDS = new BDSRegisterData();

                            puntoProcesado += 1;

                            h = 0;

                            int dataIntermedio;
                            int dataFinal;

                            while (h < REP)
                            {
                                // Solo procesaremos los datos si BDS1 = 4, 5, 6 y BDS2 = 0. Por lo que primero recuperamos los campos
                                // BDS1 y BDS2:

                                byteBDS = datosProcesando[puntoProcesado + 7];

                                BDS1 = (byteBDS & 0b11110000) >> 4;
                                BDS2 = (byteBDS & 0b00001111);

                                if ((BDS1 == 4) && (BDS2 == 0))
                                {
                                    if ((datosProcesando[puntoProcesado] & (1 << 7)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado] & 0b01111111) << 5 | (datosProcesando[puntoProcesado + 1] & 0b11111000) >> 3;
                                        dataFinal = dataIntermedio * 16;
                                        objetoBDS.MCP_FCU_selectedAltitude = dataFinal.ToString();
                                    }
                                    else
                                    {
                                        objetoBDS.MCP_FCU_selectedAltitude = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 1] & (1 << 2)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = ((datosProcesando[puntoProcesado + 1] & 0b00000011) << 10 | datosProcesando[puntoProcesado + 2] << 2 | (datosProcesando[puntoProcesado + 3] & 0b11000000) >> 6);
                                        dataFinal = dataIntermedio * 16;
                                        objetoBDS.FMS_selectedAltitude = dataFinal.ToString();
                                    }
                                    else
                                    {
                                        objetoBDS.FMS_selectedAltitude = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 3] & (1 << 5)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = ((datosProcesando[puntoProcesado + 3] & 0b00011111) << 7 | (datosProcesando[puntoProcesado + 4] & 0b11111110) >> 1);
                                        objetoBDS.barometricPressureSetting = ((dataIntermedio * 0.1) + 800).ToString();
                                    }
                                    else
                                    {
                                        objetoBDS.barometricPressureSetting = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 5] & (1 << 0)) != 0) // Comprobamos el status del campo
                                    {
                                        objetoBDS.VNAV_mode = ((datosProcesando[puntoProcesado + 6] & (1 << 7)) != 0).ToString();
                                        objetoBDS.altHold_mode = ((datosProcesando[puntoProcesado + 6] & (1 << 6)) != 0).ToString();
                                        objetoBDS.approach_mode = ((datosProcesando[puntoProcesado + 6] & (1 << 5)) != 0).ToString();
                                    }
                                    else
                                    {
                                        objetoBDS.VNAV_mode = "--";
                                        objetoBDS.altHold_mode = "--";
                                        objetoBDS.approach_mode = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 6] & (1 << 2)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 6] & 0b00000011);
                                        objetoBDS.targetAltSource = dataIntermedio.ToString();
                                    }
                                    else
                                    {
                                        objetoBDS.targetAltSource = "--";
                                    }
                                }

                                if ((BDS1 == 5) && (BDS2 == 0))
                                {
                                    if ((datosProcesando[puntoProcesado] & (1 << 7)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado] & 0b00111111) << 3 | (datosProcesando[puntoProcesado + 1] & 0b00000111);
                                        if ((datosProcesando[puntoProcesado] & (1 << 6)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.rollAngle = (-1 * dataIntermedio * (45.0 / 256)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.rollAngle = (dataIntermedio * (45.0 / 256)).ToString();
                                        }
                                            
                                    }
                                    else
                                    {
                                        objetoBDS.rollAngle = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 1] & (1 << 4)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 1] & 0b00000111) << 7 | (datosProcesando[puntoProcesado + 2] & 0b11111110) >> 1;
                                        if ((datosProcesando[puntoProcesado + 1] & (1 << 3)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.trueTrackAngle = (360 - dataIntermedio * (90.0 / 512)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.trueTrackAngle = (dataIntermedio * (90.0 / 512)).ToString();
                                        }

                                    }
                                    else
                                    {
                                        objetoBDS.trueTrackAngle = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 2] & (1 << 0)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 3] << 2) | (datosProcesando[puntoProcesado + 4] & 0b11000000) >> 6;
                                        
                                        objetoBDS.groundSpeed = (dataIntermedio * (1024 / 512)).ToString();

                                    }
                                    else
                                    {
                                        objetoBDS.groundSpeed = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 4] & (1 << 5)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 4] & 0b11110000) << 1 | (datosProcesando[puntoProcesado + 5] & 0b11111000) >> 3;
                                        if ((datosProcesando[puntoProcesado + 4] & (1 << 4)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.trackAngleRate = (-1 * dataIntermedio * (8.0 / 256)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.trackAngleRate = (dataIntermedio * (8.0 / 256)).ToString();
                                        }

                                    }
                                    else
                                    {
                                        objetoBDS.trackAngleRate = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 5] & (1 << 2)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 5] & 0b11000000) << 2 | (datosProcesando[puntoProcesado + 6]);

                                        objetoBDS.trueAirspeed = (dataIntermedio * 2).ToString();

                                    }
                                    else
                                    {
                                        objetoBDS.trueAirspeed = "--";
                                    }
                                }

                                if ((BDS1 == 6) && (BDS2 == 0))
                                {
                                    if ((datosProcesando[puntoProcesado] & (1 << 7)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado] & 0b00111111) << 4 | (datosProcesando[puntoProcesado + 1] & 0b11110000) >> 4;
                                        if ((datosProcesando[puntoProcesado] & (1 << 6)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.magnetigHeading = (360 - dataIntermedio * (90.0 / 512)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.magnetigHeading = (dataIntermedio * (90.0 / 512)).ToString();
                                        }

                                    }
                                    else
                                    {
                                        objetoBDS.magnetigHeading = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 1] & (1 << 3)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 1] & 0b00000111) << 7 | (datosProcesando[puntoProcesado + 2] & 0b11111110) >> 1;

                                        objetoBDS.indicatedAirspeed = dataIntermedio.ToString();

                                    }
                                    else
                                    {
                                        objetoBDS.indicatedAirspeed = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 2] & (1 << 0)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 3] << 2) | (datosProcesando[puntoProcesado + 4] & 0b11000000) >> 6;

                                        objetoBDS.MACH = (dataIntermedio * (2.048 / 512)).ToString();

                                    }
                                    else
                                    {
                                        objetoBDS.MACH = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 4] & (1 << 5)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 4] & 0b00001111) << 5 | (datosProcesando[puntoProcesado + 5] & 0b11111000) >> 3;
                                        if ((datosProcesando[puntoProcesado + 4] & (1 << 4)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.barometricAltitudeRate = (-1 * dataIntermedio * (8192.0 / 256)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.barometricAltitudeRate = (dataIntermedio * (8192.0 / 256)).ToString();
                                        }

                                    }
                                    else
                                    {
                                        objetoBDS.barometricAltitudeRate = "--";
                                    }

                                    if ((datosProcesando[puntoProcesado + 5] & (1 << 2)) != 0) // Comprobamos el status del campo
                                    {
                                        dataIntermedio = (datosProcesando[puntoProcesado + 5] & 0b00000001) << 8 | (datosProcesando[puntoProcesado + 6]);
                                        if ((datosProcesando[puntoProcesado + 5] & (1 << 1)) != 0) // Comprobamos si es negativo
                                        {
                                            objetoBDS.intertialVerticalVelocity = (-1 * dataIntermedio * (8192.0 / 256)).ToString();
                                        }
                                        else
                                        {
                                            objetoBDS.intertialVerticalVelocity = (dataIntermedio * (8192.0 / 256)).ToString();
                                        }

                                    }
                                    else
                                    {
                                        objetoBDS.intertialVerticalVelocity = "--";
                                    }
                                }
                                puntoProcesado += 8;
                                h++;
                            }

                            track.BDS_rData = objetoBDS;

                            break;
                        // Track Number
                        case 11:

                            track.trackNumber = (datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1]);
                            puntoProcesado += 2;

                            //Console.WriteLine($"Track number: {track.trackNumber}");

                            break;
                        // Cartesian coordinates
                        case 12:

                            cartesianCoordinates coord = new cartesianCoordinates();

                            int coordenadaX_int = (datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1]);

                            if ((datosProcesando[puntoProcesado] & 0b10000000) != 0)
                            {
                                short coordenadaX_short = (short)coordenadaX_int;
                                coord.x = (double)(convertirDeComplementoA2_short(coordenadaX_short) / 256);
                            }
                            else
                            {
                                coord.x = (double)(coordenadaX_int / 256);
                            }
                            puntoProcesado += 2;

                            int coordenadaY_int = (datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1]);

                            if ((datosProcesando[puntoProcesado] & 0b10000000) != 0)
                            {
                                short coordenadaY_short = (short)coordenadaY_int;
                                coord.y = (double)(convertirDeComplementoA2_short(coordenadaY_short) / 256);
                            }
                            else
                            {
                                coord.y = (double)(coordenadaY_int / 256);
                            }
                            puntoProcesado += 2;

                            //Console.WriteLine($"Coordenadas cartesianas: X = {coord.x}, Y = {coord.y}");

                            break;
                        // Calculated track velocity in Polar Coordinates
                        case 13:

                            track.calc_groundspeed = (double)(((datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1])) * Math.Pow(2, -14));
                            puntoProcesado += 2;
                            track.calc_heading = (double)(((datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1])) * (360/Math.Pow(2,16))); ;
                            puntoProcesado += 2;

                            //Console.WriteLine($"Velocidad de tracking en coordenadas polares: ground speed = {track.calc_groundspeed}, heading = {track.calc_heading}º");

                            break;
                        // Track status
                        case 14:

                            // Primero nos creamos un objeto de tipo trackStatus:
                            trackStatus status = new trackStatus();

                            // Empezamos a rellenar los valores:
                            status.CNF = (datosProcesando[puntoProcesado] & (1 << 7)) != 0;
                            status.RAD = (datosProcesando[puntoProcesado] & 0b01100000) >> 5;
                            status.DOU = (datosProcesando[puntoProcesado] & (1 << 4)) != 0;
                            status.MAH = (datosProcesando[puntoProcesado] & (1 << 3)) != 0;
                            status.CDM = (datosProcesando[puntoProcesado] & 0b00000110) >> 1;

                            puntoProcesado += 1;

                            if((datosProcesando[puntoProcesado] & (1 << 0)) != 0) // Comprobamos si el bit FX está activo
                            {
                                status.TRE = (datosProcesando[puntoProcesado] & (1 << 7)) != 0;
                                status.GHO = (datosProcesando[puntoProcesado] & (1 << 6)) != 0;
                                status.SUP = (datosProcesando[puntoProcesado] & (1 << 5)) != 0;
                                status.TCC = (datosProcesando[puntoProcesado] & (1 << 4)) != 0;
                            }

                            puntoProcesado += 1; // Esperamos más extensiones?

                            track.status = status;

                            break;
                        // Track Quality
                        case 15:

                            puntoProcesado += 4; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Warning/Error Conditions/Target Classification
                        case 16:

                            // Comprobamos si el bit de extensión está activo o no:
                            if ((datosProcesando[puntoProcesado] & (1 << 0)) != 0)
                            {
                                puntoProcesado += 2; // Solo lo saltamos, no hay que decodificarlo
                            }
                            else
                            {
                                puntoProcesado += 1; // Solo lo saltamos, no hay que decodificarlo
                            }

                            break;
                        // Mode-3/A Code Confidence Indicator
                        case 17:

                            puntoProcesado += 2; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Mode-C Code and Confidence Indicator
                        case 18:

                            puntoProcesado += 4; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Height Measured by 3D Radar
                        case 19:

                            int height3D_int = (datosProcesando[puntoProcesado] << 8) | (datosProcesando[puntoProcesado + 1]);

                            if ((datosProcesando[puntoProcesado] & 0b00100000) != 0)
                            {
                                short height3D_short = (short)height3D_int;
                                track.height3D = (convertirDeComplementoA2_short(height3D_short) * 25);
                            }
                            else
                            {
                                track.height3D = (height3D_int * 25);
                            }
                            puntoProcesado += 2;
                            puntoProcesado += 2;

                            //Console.WriteLine($"Altura medida por el radar 3D: {track.height3D}ft");

                            break;
                        // Radial doppler speed
                        case 20:

                            // En este caso, tendremos un byte fijo, y luego puede ser 2 bytes o 7 bytes

                            if ((datosProcesando[puntoProcesado] & 0b10000000) != 0)
                            {
                                puntoProcesado += 2;
                            }
                            else if ((datosProcesando[puntoProcesado] & 0b01000000) != 0)
                            {
                                puntoProcesado += 7;
                            }

                            puntoProcesado += 1;

                            break;
                        // Communications / ACAS Capability and Flight Status
                        case 21:

                            // Primero nos creamos un objeto de tipo trackStatus:
                            acasStatus a_status = new acasStatus();

                            // Empezamos a rellenar los valores:
                            a_status.COM = (datosProcesando[puntoProcesado] & 0b11100000) >> 5;
                            a_status.STAT = (datosProcesando[puntoProcesado] & 0b00011100) >> 2;
                            a_status.SI = (datosProcesando[puntoProcesado] & (1 << 1)) != 0;
                            a_status.MSSC = (datosProcesando[puntoProcesado + 1] & (1 << 7)) != 0;
                            a_status.ARC = (datosProcesando[puntoProcesado + 1] & (1 << 6)) != 0;
                            a_status.AIC = (datosProcesando[puntoProcesado + 1] & (1 << 5)) != 0;
                            a_status.B1A = (datosProcesando[puntoProcesado + 1] & (1 << 4)) != 0;
                            a_status.B1B = (datosProcesando[puntoProcesado + 1] & 0b00001111);

                            puntoProcesado += 2;

                            track.a_status = a_status;

                            break;
                        // ACAS Resolution Advisory Report
                        case 22:

                            puntoProcesado += 7; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Mode-1 Code in Octal Representation
                        case 23:

                            puntoProcesado += 1; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Mode-2 Code in Octal Representation
                        case 24:

                            puntoProcesado += 2; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Mode-1 Code Confidence Indicator
                        case 25:

                            puntoProcesado += 1; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Mode-2 Code Confidence Indicator
                        case 26:

                            puntoProcesado += 2; // Solo lo saltamos, no hay que decodificarlo

                            break;
                        // Special Purpose Field
                        case 27:
                            //Console.WriteLine("Caso 27: Special Purpose Field");

                            break;
                        // Reserved Expansion Field
                        case 28:
                            //Console.WriteLine("Caso 28: Reserved Expansion Field");

                            break;
                        default:
                            //Console.WriteLine("Data field fuera de rango");
                            //Console.WriteLine("Presiona Enter para continuar...");
                            //Console.ReadLine(); // Espera a que se presione Enter

                            break;
                    }
                }

                // Finalmente nos calculamos el valor de altitud real, y si estamos por debajo de 6000ft realizamos la corrección:

                if (track.flightLevel < 60)
                {
                    if(track.BDS_rData.barometricPressureSetting == "--" || track.BDS_rData.barometricPressureSetting == "")
                    {
                        track.realAltitude = track.flightLevel * 100 + (1013.25 - 1013.25) * 30;
                    }
                    else
                    {
                        track.realAltitude = track.flightLevel * 100 + (Convert.ToDouble(track.BDS_rData.barometricPressureSetting) - 1013.25) * 30;
                    }
                    
                }
                else
                {
                    track.realAltitude = track.flightLevel * 100;
                }

                CoordinatesWGS84 aircraftCoordinates = new CoordinatesWGS84();

                geodesicCoordinates track_geodesicCoordinates = new geodesicCoordinates();

                aircraftCoordinates = coordinatesTransformation(track.rho_polar, track.theta_polar, track.realAltitude);

                track_geodesicCoordinates.latitude = aircraftCoordinates.Lat*(180 / Math.PI);
                track_geodesicCoordinates.longitude = aircraftCoordinates.Lon * (180 / Math.PI);
                track_geodesicCoordinates.height = aircraftCoordinates.Height;

                track.coordenadasGeodesicas = track_geodesicCoordinates;

                listaTracks.Add(track);
                i++;
            }

            //Una vez ha finalizado toda la ejecución, procedemos a crearnos el archivo CSV:
            try
            {
                File.Delete("C:\\Users\\hector\\Desktop\\data.csv");
            }
            catch
            {

            }
            string filePath = "C:\\Users\\hector\\Desktop\\data.csv";
            WriteToCSV(listaTracks, listaDataRecords, filePath);

            Console.WriteLine("Presiona Enter para continuar...");
            Console.ReadLine(); // Espera a que se presione Enter

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

        static int convertirDeComplementoA2_short(int byteComplementoA2)
        {
            int valor_convertido = 1; // Si la función acaba devolviendo 1, hay un error puesto que debe ser negativo;

            // Antes de nada, separamos el short en dos bytes, para poder invertirlos:

            byte byteMSB = (byte)(((short)byteComplementoA2 & 0b1111111100000000) >> 8);
            byte byteLSB = (byte)(((short)byteComplementoA2 & 0b0000000011111111));

            // Primero invertimos todos los bits y le sumamos 1 al LSB:
            byte intermedioMSB = (byte)~byteMSB;
            intermedioMSB = (byte)(intermedioMSB & 0b00011111); // Le quitamos los bits que no formaban parte de la información
            byte intermedioLSB = (byte)~byteLSB;
            intermedioLSB += 1;


            int final = (intermedioMSB << 8) | intermedioLSB;

            valor_convertido = (-1) * final;

            return valor_convertido;
        }

        public static void WriteToCSV(List<trackInfo_struct> listaTracks, List<dataRecord_struct> listaDataRecords, string filePath)
        {
            if (listaTracks == null || !listaTracks.Any() || listaDataRecords == null || !listaDataRecords.Any())
            {
                Console.WriteLine("La lista está vacía o nula.");
                return;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                   // Escribimos los títulos de las columnas del archivo CSV
                    /*writer.WriteLine("# Data record;CAT;LEN;data LEN;SIC;SAC;Time;TYP;SIM;RDP;SPI;RAB;TST;ERR;XPP;ME;MI;FOE/FRI;ADSB;" +
                        "SCN;PAI;Polar coor: rho [NM};Polar coord: theta [deg];Mode-3/A Code not Validated;Mode-3/A Garbled Code;" +
                        "Mode-3/A not from the last scan;Mode-3/A_Code;Flight Level Code not Validated;Flight Level Garbled code;Flight Level;" +
                        "SSR plot runlength [deg];Number of received replies for M(SSR);Amplitud of M(SSR) reply [dBm];Primary plot runlength [deg];" +
                        "Amplitude of primary plot [dBm];Difference in range between PSR and SSR plot [NM];Difference in azimuth between PSR and SSR plot [deg];" +
                        "Aircraft address;Aircraft identification;" + BDSregisterDataString + ";Track number;Cartesian coord: x [NM];Cartesian coord: y [NM];" +
                        "Groundspeed [kt];Heading [deg];CNF;RAD;DOU;MAH;CDM;TRE;GHO;SUP;TCC;Height measured (3D radar) [ft];COM;STAT;SI;MSSC;ARC;" +
                        "AIC;B1A;B1B");*/

                    writer.WriteLine("# Data record;CAT;LEN;data LEN;SIC;SAC;Time;[;*;*;*;*;*;Target Report Descriptor;*;*;*;*;*;" +
                        "*;];Polar coor: rho [NM};Polar coord: theta [deg];Mode-3/A Code not Validated;Mode-3/A Garbled Code;" +
                        "Mode-3/A not from the last scan;Mode-3/A_Code;Flight Level Code not Validated;Flight Level Garbled code;Flight Level;" +
                        "SSR plot runlength [deg];Number of received replies for M(SSR);Amplitud of M(SSR) reply [dBm];Primary plot runlength [deg];" +
                        "Amplitude of primary plot [dBm];Difference in range between PSR and SSR plot [NM];Difference in azimuth between PSR and SSR plot [deg];" +
                        "Aircraft address;Aircraft identification;[;*;*;BDS code 4,0 (Selected Vertical Information);*;*;];[;*;BDS code 5,0 (Track and Turn Report);*;];" +
                        "[;*;BDS code 6,0 (Heading and Speed Report);*;];Track number;Cartesian coord: x [NM];Cartesian coord: y [NM];" +
                        "Groundspeed [kt];Heading [deg];[;*;*;*;Track Status;*;*;*;];Height measured (3D radar) [ft];[;*;*;ACAS Capability and Flight Satus;*;" +
                        "*;*;];Corrected altitude;[;Geodesic coordinates;]");

                    writer.WriteLine(";;;;;;;TYP;SIM;RDP;SPI;RAB;TST;ERR;XPP;ME;MI;FOE/FRI;ADSB;" +
                        "SCN;PAI;;;;;;;;;;;;;;;;;;" +
                        ";MCP/FCU Selected Altitude;FMS Selected Altitude;Barometric Pressure Setting;VNAV mode;ALT HOLD mode;Approach mode;Target Altitude Source;Roll Angle;" +
                        "True Track Angle;Ground Speed;Track Angle Rate;True Airspeed;Magnetic Heading;Indicated Airspeed;Mach;Barometric Altitude Rate;Intertial Vertical Velocity;;;;" +
                        ";;CNF;RAD;DOU;MAH;CDM;TRE;GHO;SUP;TCC;;COM;STAT;SI;MSSC;ARC;" +
                        "AIC;B1A;B1B;;Latitude;Longitude;Altitude");

                    //Ahora rellenamos las filas con los distintos tracks:
                    int i = 1;
                    foreach (trackInfo_struct track in listaTracks)
                    {
                        dataRecord_struct dataRecordEscribiendo = listaDataRecords[i - 1];
                        string line = $"{i};{dataRecordEscribiendo.cat};{dataRecordEscribiendo.longitud};{dataRecordEscribiendo.datos.Length};" +
                            $"{track.SIC};{track.SAC};{track.timeOfDay};{track.TRD.TYP};{track.TRD.SIM};{track.TRD.RDP};{track.TRD.SPI};{track.TRD.RAB};" +
                            $"{track.TRD.TST};{track.TRD.ERR};{track.TRD.XPP};{track.TRD.ME};{track.TRD.MI};{track.TRD.FOE_FRI};{track.TRD.ADSB};" +
                            $"{track.TRD.SCN};{track.TRD.PAI};{track.rho_polar};{track.theta_polar};{track.mode3A_V};{track.mode3A_G};{track.mode3A_L};" +
                            $"{track.mode3A_code};{track.flightLevel_V};{track.flightLevel_G};{track.flightLevel};{track.RPC.SRL};{track.RPC.SRR};" +
                            $"{track.RPC.SAM};{track.RPC.PRL};{track.RPC.PAM};{track.RPC.RPD};{track.RPC.APD};{track.AC_address};" +
                            $"{track.AC_identification};{track.BDS_rData.MCP_FCU_selectedAltitude};{track.BDS_rData.FMS_selectedAltitude};" +
                            $"{track.BDS_rData.barometricPressureSetting};{track.BDS_rData.VNAV_mode};{track.BDS_rData.altHold_mode};{track.BDS_rData.approach_mode};" +
                            $"{track.BDS_rData.targetAltSource};{track.BDS_rData.rollAngle};{track.BDS_rData.trueTrackAngle};{track.BDS_rData.groundSpeed};" +
                            $"{track.BDS_rData.trackAngleRate};{track.BDS_rData.trueAirspeed};{track.BDS_rData.magnetigHeading};{track.BDS_rData.indicatedAirspeed};" +
                            $"{track.BDS_rData.MACH};{track.BDS_rData.barometricAltitudeRate};{track.BDS_rData.intertialVerticalVelocity};" +
                            $"{track.trackNumber};{track.cartesianCoord.x};{track.cartesianCoord.y};{track.calc_groundspeed};" +
                            $"{track.calc_heading};{track.status.CNF};{track.status.RAD};{track.status.DOU};{track.status.MAH};{track.status.CDM};{track.status.TRE};" +
                            $"{track.status.GHO};{track.status.SUP};{track.status.TCC};{track.height3D};{track.a_status.COM};{track.a_status.STAT};" +
                            $"{track.a_status.SI};{track.a_status.MSSC};{track.a_status.ARC};{track.a_status.AIC};{track.a_status.B1A};{track.a_status.B1B};" +
                            $"{track.realAltitude};{track.coordenadasGeodesicas.latitude};{track.coordenadasGeodesicas.longitude};{track.coordenadasGeodesicas.height}";
                        writer.WriteLine(line);
                        i++;
                    }
                }

                Console.WriteLine("Los datos se han escrito en el archivo CSV correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Se produjo un error al escribir en el archivo CSV: {ex.Message}");
            }
        }

        public static CoordinatesWGS84 coordinatesTransformation(double range, double azimuth, double aircraft_altitude) // Luego cmbiarlo para que saque una estructura de tipo coordenadas
        {

            // Antes de nada introducimos algunas constantes:
            GeoUtils conversiones = new GeoUtils();

            CoordinatesWGS84 coordenadasRadar = new CoordinatesWGS84(0.720833239, 0.0366878365, 2032.25);
            double R_Ti = conversiones.CalculateEarthRadius(coordenadasRadar); // Radio de la tierra en la posición del radar. EL QUE HAY AHORA ES EL MEDIO
            double h_Ri = 2.007 + 25.25; // Altitud del radar. Hemos sumado la elevación a la altura de la antena

            // En primer lugar nos calcularemos la elevación del target:
            double numerador = 2 * R_Ti * (aircraft_altitude - h_Ri) + Math.Pow(aircraft_altitude, 2) - Math.Pow(h_Ri, 2) - Math.Pow(range, 2);
            double denominador = 2*range*(R_Ti+h_Ri);
            double elevation = GeoUtils.CalculateElevation(coordenadasRadar, R_Ti, range * 1852, aircraft_altitude * 0.305);

            CoordinatesPolar coordenadasPolaresRadar = new CoordinatesPolar(range*1852, azimuth * (Math.PI/180), elevation);

            // Una vez tenemos la elevación, procedemos a hacer los correspondientes cambios de coordenadas con ayuda del GeoUtils:
            CoordinatesXYZ coordenadasCartesianasRadar = GeoUtils.change_radar_spherical2radar_cartesian(coordenadasPolaresRadar);
            CoordinatesXYZ coordenadasGeocentricas = conversiones.change_radar_cartesian2geocentric(coordenadasRadar, coordenadasCartesianasRadar);
            CoordinatesWGS84 coordenadasGeodesicas = conversiones.change_geocentric2geodesic(coordenadasGeocentricas);

            return coordenadasGeodesicas;
        }

    }

}
