using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;

namespace SunCoordinatesCalculator {
    class Program {

        struct Data
        {
            public float latitud;
            public float longitud;
            public float timeZone;
            public DateTime fecha;
            public List<float> hora;
            public DateTime origen;
            public float numNow;
            public float numHora;
        }


        static float Date2num(DateTime now, DateTime origen, int orgNum = 36273) {
            TimeSpan dif1 = now - origen;
            float dif2 = dif1.Days;
            return orgNum + dif2;
        }

        static float Hora2dd(List<float> hora) {
            float g = hora[0]; float m = hora[1]; float s = hora[2];
            float num = ((g * 3600f) + (m * 60f) + s) / 86400f;
            return num;
        }

        static List<float> Dd2dg(double dd) {
            float mnt = (float)Math.Truncate(((dd * 3600f) / 60f));
            float sec = (float)((dd * 3600f) % 60f);
            float deg = (float)Math.Truncate(mnt / 60);
            float mnt2 = mnt % 60;
            return new List<float> { deg, mnt2, (float)Math.Round(sec, 2) };
        }

        static List<double> SunCoordinates(float latitud, float longitud, float timeZone, float numNow, float numHora) {
            double DJ = Math.Round(numNow + 2415018.5 + numHora - (timeZone / 24f), 2);
            double SJ = Math.Round((DJ - 2451545) / 36525, 7);
            double L = (280.46646 + (SJ * (36000.76983 + (SJ * 0.0003032)))) % 360;
            double g = 357.52911 + SJ * (35999.05029 - 0.0001537 * SJ);
            double exc = 0.016708634 - SJ * (0.000042037 + 0.0000001267 * SJ);
            double g_rad = g * (Math.PI / 180);
            double eqCtr = Math.Sin(g_rad) * (1.914602 - SJ * (0.004817 + 0.000014 * SJ)) + Math.Sin(2 * g_rad) * (0.019993 - 0.000101 * SJ) + Math.Sin(3 * g_rad) * 0.000289;
            double L_true = L + eqCtr;
            double g_true = g + eqCtr;
            double radVec = (1.000001018 * (1 - Math.Pow(exc, 2))) / (1 + exc * Math.Cos(g_true * (Math.PI / 180)));
            double aux1 = (125.04 - 1934.136 * SJ) * (Math.PI / 180);
            double L_ap = L_true - 0.00569 - 0.00478 * Math.Sin(aux1);
            double oblElip = 23 + (26 + ((21.448 - SJ * (46.815 + SJ * (0.00059 - SJ * 0.001813)))) / 60) / 60;
            double aux2 = (125.04 - 1934.136 * SJ) * (Math.PI / 180);
            double oblCorreg = oblElip + 0.00256 * Math.Cos(aux2);
            double ascP1 = Math.Cos(L_ap * (Math.PI / 180));
            double ascP2 = Math.Cos(oblCorreg * (Math.PI / 180)) * Math.Sin(L_ap * (Math.PI / 180));
            double ascRect = Math.Atan2(ascP2, ascP1) * (180 / Math.PI);
            double decP1 = Math.Sin(oblCorreg * (Math.PI / 180));
            double decP2 = Math.Sin(L_ap * (Math.PI / 180));
            double Decl = Math.Asin(decP1 * decP2) * (180 / Math.PI);
            double VarP1 = (oblCorreg / 2) * (Math.PI / 180);
            double varY = Math.Tan(VarP1) * Math.Tan(VarP1);
            double EDT1 = Math.Sin(2 * (L * (Math.PI / 180)));
            double EDT2 = Math.Sin(g * (Math.PI / 180));
            double EDT3 = Math.Sin(g * (Math.PI / 180));
            double EDT4 = Math.Cos(2 * (L * (Math.PI / 180)));
            double EDT5 = Math.Sin(4 * (L * (Math.PI / 180)));
            double EdT = 4 * ((varY * EDT1 - 2 * exc * EDT2 + 4 * exc * varY * EDT3 * EDT4 - 0.5 * varY * varY * EDT5 - 1.25 * exc * exc * EDT3) * (180 / Math.PI));
            double TST1 = (numHora * 1440) + EdT + (4 * longitud) - (60 * timeZone);
            double TST = TST1 % 1440;
            double HA;
            if (TST / 4 < 0) {
                HA = (TST / 4) + 180;
            } else {
                HA = (TST / 4) - 180;
            }
            double Z1 = Math.Sin(latitud * (Math.PI / 180));
            double Z2 = Math.Sin(Decl * (Math.PI / 180));
            double Z3 = Math.Cos(latitud * (Math.PI / 180));
            double Z4 = Math.Cos(Decl * (Math.PI / 180));
            double Z5 = Math.Cos(HA * (Math.PI / 180));
            double zenith = Math.Acos(Z1 * Z2 + Z3 * Z4 * Z5) * (180 / Math.PI);
            double Elevacion = 90 - zenith;
            double AtmRef;
            if (Elevacion > 85) {
                AtmRef = 0;
            } else {
                if (Elevacion > 5) {
                    AtmRef = 58.1 / Math.Tan(Elevacion * (Math.PI / 180)) - 0.07 / Math.Tan(Math.Pow(Elevacion * (Math.PI / 180), 3)) +
                        0.000086 / Math.Tan(Math.Pow((Elevacion * (Math.PI / 180)), 5));
                } else {
                    if (Elevacion > 0.575) {
                        AtmRef = 1735 + Elevacion * (-518.2 + Elevacion * (103.4 + Elevacion * (-12.79 + Elevacion * 0.711)));
                    } else {
                        AtmRef = -20.772 / Math.Tan(Elevacion * (Math.PI / 180));
                    }
                }
            }
            AtmRef = AtmRef / 3600;
            double Elevacion_c = Elevacion + AtmRef;
            double Azimuth;
            if (HA > 0) {
                Azimuth = (((Math.Acos(((Math.Sin((latitud) * (Math.PI / 180)) * Math.Cos((zenith) * (Math.PI / 180))) -
                    (Math.Sin((Decl) * (Math.PI / 180)))) / (Math.Cos((latitud) * (Math.PI / 180)) * Math.Sin((zenith) *
                    (Math.PI / 180))))) * (180 / Math.PI)) + 180) % 360;
            } else {
                Azimuth = (540 - ((Math.Acos(((Math.Sin((latitud) * (Math.PI / 180)) * Math.Cos((zenith) * (Math.PI / 180))) -
                    (Math.Sin((Decl) * (Math.PI / 180)))) / (Math.Cos((latitud) * (Math.PI / 180)) * Math.Sin((zenith) *
                    (Math.PI / 180))))) * (180 / Math.PI))) % 360;
            }
            
            return new List<double> { Azimuth, Elevacion_c, ascRect, Decl };
        }

        static bool EsBisisto(int anyo) {
            if (((anyo % 4 == 0) & (anyo % 100 != 0)) | (anyo % 400 == 0)) {
                return true;
            }
            return false;
        }

        static Data Datos() {

            Data values = new Data();

            Console.WriteLine("Introduce los datos:\n--------------------------\n");

            values.latitud = 700f;
            while (values.latitud < -90f || values.latitud > 90f) {
                Console.Write("Latitud [x,xxxx°] (+ N, - S): ");
                values.latitud = (float)Convert.ToDecimal(Console.ReadLine());
                if (values.latitud < -90f || values.latitud > 90f) {
                    Console.WriteLine("ERROR. VALOR INCORRECTO.\nLA LATITUD DEBE SER UN NÚMERO DECIMAL ENTRE -90 Y 90");
                }
            }

            values.longitud = -200f;
            while (values.longitud < -180f || values.longitud > 180f) {
                Console.Write("Longitud [x,xxxx°] (+ E, - W): ");
                values.longitud = (float)Convert.ToDecimal(Console.ReadLine());
                if (values.longitud < -180f || values.longitud > 180f) {
                    Console.WriteLine("ERROR. VALOR INCORRECTO.\nLA LONGITUD DEBE SER UN NÚMERO DECIMAL ENTRE -180 Y 180");
                }
            }
            
            values.timeZone = -20f;
            while (values.timeZone < -11f || values.timeZone > 12f) {
                Console.Write("Zona Horaria [UTC = 0] (+ E, - W): ");
                values.timeZone = (float)Convert.ToDecimal(Console.ReadLine());
                if (values.timeZone < -11f || values.timeZone > 12f) {
                    Console.WriteLine("ERROR. VALOR INCORRECTO.\nLA ZONA HORARIA DEBE SER UN NÚMERO ENTERO ENTRE -12 Y 12");
                }
            }

            int dia;
            int mes;
            int anyo;
            int cond = 0;   
            while (cond == 0) {
                Console.Write("Introduce la fecha [YYYY/mm/dd]: ");
                string str = Console.ReadLine();
                string[] split = str.Split(new Char[] { '/' });
                anyo = Convert.ToInt32(split[0]); mes = Convert.ToInt32(split[1]); dia = Convert.ToInt32(split[2]);
                try {
                    values.fecha = new DateTime(anyo, mes, dia);
                    cond = 1;
                }
                catch (ArgumentOutOfRangeException) {
                    if (anyo < 0)
                    {
                        Console.WriteLine("ERROR. EL AÑO DEBE SER POSITIVO (DESPUÉS DE CRISTO).");
                    }
                    else if (mes < 1 | mes > 12)
                    {
                        Console.WriteLine("Introduce un mes válido [1-12]: ");
                    }
                    else if ((EsBisisto(anyo)) & (mes == 2) & (dia > 29 | dia < 1)) {
                        Console.WriteLine("ERROR. LA FECHA INTRODUCIDA NO ES VÁLIDA. FEBEROR DE UN AÑO BISIESTO TIENE 29 DIAS.");
                    } else {
                        Console.WriteLine("ERROR. EL DÍA ES INCORRECTO, COMPRUEBA LA FECHA.");
                    }
                }

            }

            float H = -1;
            float M = -1;
            float S = -1;
            while ((H < 0 | H > 24) & (M < 0 | M > 59) & (S < 0 | S > 59)) {
                Console.Write("Introduce la hora (hh:mm:ss) 24H: ");
                string str = Console.ReadLine();
                string[] split = str.Split(new Char[] { ':' });
                H = (float)Convert.ToDecimal(split[0]); M = (float)Convert.ToDecimal(split[1]); S = (float)Convert.ToDecimal(split[2]);
                values.hora = new List<float> { H, M, S };
            }

            values.origen = new DateTime(1999, 4, 23);

            values.numNow = Date2num(values.fecha, values.origen);

            values.numHora = Hora2dd(values.hora);

            return values;
        }

        static string[] Localizacion(double latitud, double longitud) {
            string dirLat;
            string dirLon;
            if (latitud >= 0) { dirLat = "N"; } else { dirLat = "S"; }
            if (longitud >= 0) { dirLon = "E"; } else { dirLon = "W"; }

            List<float> lat = Dd2dg(latitud); List<float> lon = Dd2dg(longitud);

            string LAT = lat[0] + "° " + lat[1] + "' " + lat[2] + "\" " + dirLat;
            string LON = lon[0] + "° " + lon[1] + "' " + lon[2] + "\" " + dirLon;

            return new string[] { LAT, LON };
        }

        static void Calculadora() {
            Console.Clear();
            string[] meses = new string[] { "Enero", "Febrero", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            Data datos = Datos();
            List<double> res = SunCoordinates(datos.latitud, datos.longitud, datos.timeZone, datos.numNow, datos.numHora);
            Console.Clear();
            float g = datos.hora[0]; float m = datos.hora[1]; float s = datos.hora[2];
            string G = String.Format("{0:00}", g); string M = String.Format("{0:00}", m); string S = String.Format("{0:00}", s);
            Console.WriteLine("Coordenadas del Sol para el día " + datos.fecha.Day + " de " + meses[datos.fecha.Month - 2] + " del " + datos.fecha.Year + " a las " + G + ":" + M + ":" + S);
            string[] coords = Localizacion(datos.latitud, datos.longitud);
            Console.WriteLine("Localización: " + coords[0] + "   " + coords[1]);
            Console.WriteLine("");
            double az = res[0]; double alt = res[1];
            List<float> AZ = Dd2dg(az); List<float> ALT = Dd2dg(alt);
            Console.WriteLine("SISTEMA AZIMUTAL");
            Console.WriteLine("Azimuth    = " + AZ[0] + "° " + AZ[1] + "' " + AZ[2] + "\"");
            Console.WriteLine("Elevación = " + ALT[0] + "° " + ALT[1] + "' " + ALT[2] + "\"");
        }

        static void Main() {
            Console.Title = "Calculadora de las coordenadas del Sol";
            Calculadora();
        }
    }
}
