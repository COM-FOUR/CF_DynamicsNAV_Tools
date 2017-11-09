namespace Com.SharpZebra.Printing
{
    public interface IZebraPrinter
    {
        void Print(byte[] Data);
        PrinterSettings Settings { get; set; }     
    }

    public class PrinterSettings
    {
        public int id { get; set; }
        public char PrinterType { get; set; }
        public string PrinterName { get; set; }
        public int PrinterPort { get; set; }
        public int AlignLeft { get; set; }
        public int AlignTop { get; set; }
        public int AlignTearOff { get; set; }
        public int Darkness { get; set; }
        public int PrintSpeed { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public char RamDrive { get; set; }

        public PrinterSettings()
        {
            RamDrive = 'R';
        }
    }
}