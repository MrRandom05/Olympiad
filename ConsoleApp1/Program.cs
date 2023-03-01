namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            void GetRectangleData(int width, int height, out int rectArea, out int rectPerimetr)
            {
                rectArea = width * height;
                rectPerimetr = (width + height) * 2;
            }

            GetRectangleData(10, 20, out int area, out int perimetr);

            Console.WriteLine($"Площадь прямоугольника: {area}");       // 200
            Console.WriteLine($"Периметр прямоугольника: {perimetr}");   // 60



            Console.WriteLine("Hello, World!");
        }
    }
}