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


    public struct trackInfo_struct
    {
        public int SAC { get; set; }
        public int SIC { get; set; }
        public time timeOfDay { get; set; }
        public targetReportDescriptor TRD { get; set; }

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
                trackInfo_struct track = new trackInfo_struct();
                dataRecordProcesando = listaDataRecords[i];

                //Ahora vamos rellenando los distintos campos del objeto track:

                /*public int SAC { get; set; }
                public int SIC { get; set; }
                public double time { get; set; }
                public int TYP { get; set; }
                public Boolean simulated { get; set; }
                public Boolean RDP_chain2 { get; set; }
                public Boolean SPI { get; set; }
                public Boolean fieldMonitor { get; set; }
                public Boolean testTarget { get; set; }
                public Boolean extendedRange { get; set; }
                public Boolean xPulse { get; set; }
                public Boolean military { get; set; }
                public int FOE_FRI { get; set; }*/

                //El procedimiento será el mismo siempre, comprobamos si está el data field que nos interesa, y luego cojemos la info

                //SAC y SIC los obtenemos del data field 10:

                //Primero hemos de comprobar que el FSPEC nos indique que si existe el field 10:

                /*int comprueba = 10;
                decimal d = comprueba / 8;
                int byteInteres = Decimal.ToInt32(d);

                if (byteInteres <= dataRecordProcesando.long_fspec) //Comprobamos que el FSPEC llegue hasta el dafield 10
                {
                    int bitInteres = comprueba - 7 * byteInteres;
                    bool siDataField = (dataRecordProcesando.fspec[byteInteres] & (1 << bitInteres)) != 0;
                    if (siDataField)
                    {
                        //Si está a 1 el bit del datafield correspondiente, procedemos a coger la información:


                    }
                }*/

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
                foreach (int numero in dataFields_presentes)
                {
                    switch (numero)
                    {
                        case 1: //Data source identifier
                            Console.WriteLine("Caso 1");
                            track.SAC = datosProcesando[puntoProcesado];
                            track.SIC = datosProcesando[puntoProcesado + 1];

                            puntoProcesado += 2;

                            break;
                        case 2: //Time of day
                            Console.WriteLine("Caso 2");

                            byte byteMSB = datosProcesando[puntoProcesado];
                            byte byteMedio = datosProcesando[puntoProcesado+1];
                            byte byteLSB = datosProcesando[puntoProcesado+2];

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
                        case 3: // Target report descriptor
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

                            byte mascara = 0b11100000;
                            byte bitsDeInteres = (byte)(datosProcesando[puntoProcesado] & mascara); // Aplicamos la máscara

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
                                break;
                            }

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
                                break;
                            }



                            track.TRD = trd;
                            break;
                        case 4:
                            Console.WriteLine("Caso 4");
                            break;
                        case 5:
                            Console.WriteLine("Caso 5");
                            break;
                        case 6:
                            Console.WriteLine("Caso 6");
                            break;
                        case 7:
                            Console.WriteLine("Caso 7");
                            break;
                        case 8:
                            Console.WriteLine("Caso 8");
                            break;
                        case 9:
                            Console.WriteLine("Caso 9");
                            break;
                        case 10:
                            Console.WriteLine("Caso 10");
                            break;
                        case 11:
                            Console.WriteLine("Caso 11");
                            break;
                        case 12:
                            Console.WriteLine("Caso 12");
                            break;
                        case 13:
                            Console.WriteLine("Caso 13");
                            break;
                        case 14:
                            Console.WriteLine("Caso 14");
                            break;
                        case 15:
                            Console.WriteLine("Caso 15");
                            break;
                        case 16:
                            Console.WriteLine("Caso 16");
                            break;
                        case 17:
                            Console.WriteLine("Caso 17");
                            break;
                        case 18:
                            Console.WriteLine("Caso 18");
                            break;
                        case 19:
                            Console.WriteLine("Caso 19");
                            break;
                        case 20:
                            Console.WriteLine("Caso 20");
                            break;
                        case 21:
                            Console.WriteLine("Caso 21");
                            break;
                        case 22:
                            Console.WriteLine("Caso 22");
                            break;
                        case 23:
                            Console.WriteLine("Caso 23");
                            break;
                        case 24:
                            Console.WriteLine("Caso 24");
                            break;
                        case 25:
                            Console.WriteLine("Caso 25");
                            break;
                        case 26:
                            Console.WriteLine("Caso 26");
                            break;
                        case 27:
                            Console.WriteLine("Caso 27");
                            break;
                        case 28:
                            Console.WriteLine("Caso 28");
                            break;
                        default:
                            Console.WriteLine("Data field fuera de rango");
                            break;
                    }
                }
            }
        }
    }
}
