using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using OSMLSGlobalLibrary;
using OSMLSGlobalLibrary.Map;
using OSMLSGlobalLibrary.Modules;

namespace TestModule
{
    public class TestModule : OSMLSModule
    {
        // Координаты аэропортов разных стран
        private readonly Coordinate china = new Coordinate(13281923, 2999641);
        private readonly Coordinate moscow = new Coordinate(4148531, 7478939);
        private readonly Coordinate iraq = new Coordinate(4853670, 3939752);
        private readonly Coordinate afganistan = new Coordinate(7704360, 4105115);
        private readonly Coordinate usa = new Coordinate(-8523466, 4713417);
        //Списки где будут храниться все аэропорты и самолеты
        List<Airport> allAirports = new List<Airport>();
        List<Airplane> allAirplanes = new List<Airplane>();
        protected override void Initialize()
        {
            //Добавление созданных объектов в общий список, доступный всем модулям. Объекты из данного списка отображаются на карте. 
            MapObjects.Add(new Airport(china, 20));
            MapObjects.Add(new Airport(moscow, 13));
            MapObjects.Add(new Airport(iraq, 9));
            MapObjects.Add(new Airport(afganistan, 7));
            MapObjects.Add(new Airport(usa, 17));
            allAirports = MapObjects.GetAll<Airport>();
        }
        //Создание самолета на карте
        public void CreateAirplane(Airplane airpl)
        {
            MapObjects.Add(airpl);
        }
        //Удаление самолета с карты
        public void RemoveAirplane(Airplane obj)
        {
            MapObjects.Remove(obj);
        }
        //Создание самолетов в аэропорте, если они там существуют
        public void CreateAirplanes()
        {
            foreach (var airport in allAirports)
            {
                if (airport.Count != 0)
                {
                    CreateAirplane(airport.FlyOff(allAirplanes.Count, allAirports[new Random().Next(0, 5)]));
                    allAirplanes = MapObjects.GetAll<Airplane>();
                }
            }
        }
        //Движение самолетов в их пункты назначения-аэропорты и их удаление при достижении пункта назначения
        public void Fly()
        {
            foreach (var airplane in MapObjects.GetAll<Airplane>())
            {
                int flew = airplane.MoveToAirport();
                if (flew != -1)
                {
                    RemoveAirplane(airplane);
                }
            }
        }
        public override void Update(long elapsedMilliseconds)
        {
            CreateAirplanes();
            Fly();
        }
    }
    // Самолет, умеющий летать в назначенный аэропорт с заданной скоростью
    [CustomStyle(
        @"new ol.style.Style({ 
            image: new ol.style.Circle({ 
                opacity: 1.0, 
                scale: 1, 
                radius: 2, 
                fill: new ol.style.Fill({ 
                    color: 'rgba(0, 255, 0, 0.6)' 
                }), 
                stroke: new ol.style.Stroke({ 
                    color: 'rgb(0, 0, 0)', 
                    width: 1 
                }), 
            }) 
        }); 
        ")]
    public class Airplane : Point // Унаследуем данный данный класс от стандартной точки. 
    {
        // Скорость самолета. 
        public double Speed { get; set; }
        // Координаты самолета.
        public Coordinate airplaneCoord { get; set; }
        // false, если самолет не достиг назначенного аэропорта, иначе true.
        public bool inAirport { get; set; }
        // id самолета.
        public int idAirplane { get; set; }
        // Пункт назначения-аэропорт.
        public Airport destAirport { get; set; }
        // Конструктор для создания нового объекта. 
        public Airplane(Coordinate coordinate, double speed, bool inairport, int id, Airport destination) : base(coordinate)
        {
            Speed = speed;
            airplaneCoord = coordinate;
            inAirport = inairport;
            idAirplane = id;
            destAirport = destination;
        }
        // Двигает самолет в аэропорт. При достижении самолета пункта назначения-аэропорта возвращает id самолета, увеличивает количество самолетов в аэропорте,
        // устанавливает inAirport=true, иначе возвращает -1.
        internal int MoveToAirport()
        {
            //Каноническое уравнение прямой.
            var x1 = airplaneCoord.X;
            var y1 = airplaneCoord.Y;
            var x2 = destAirport.airportCoord.X;
            var y2 = destAirport.airportCoord.Y;
            var x = airplaneCoord.X;
            if (x1 < x2)
            {
                x += Speed;
                if ((x2 - x1) < Speed)
                {
                    x += x2 - x1;
                }
}
            if (x1 > x2)
            {
                x -= Speed;
                if ((x1 - x2) < Speed)
                {
                    x += x2 - x1;
                }
            }
            if (airplaneCoord.X == x2 && airplaneCoord.Y == y2)
            {
                Console.WriteLine("Долетел до " + destAirport);
                destAirport.Count++;
                inAirport = true;
                return idAirplane;
            }
            X = x;
            Y = ((y2* (x - x1)) - (y1* (x - x2))) / (x2 - x1);
            return -1;
        }
    }
    // Аэропорт, умеющий отправлять самолеты в другие аэропорты.
    [CustomStyle(
        @"new ol.style.Style({ 
            image: new ol.style.Circle({ 
                opacity: 1.0, 
                scale: 1.5, 
                radius: 5, 
                fill: new ol.style.Fill({ 
                    color: 'rgba(150, 0, 255, 0.6)' 
                }), 
                stroke: new ol.style.Stroke({ 
                    color: 'rgb(0, 0, 0)', 
                    width: 1 
                }), 
            }) 
        }); 
        ")]
    public class Airport : Point // Унаследуем данный данный класс от стандартной точки. 
    {
        // Количество самолетов в аэропорте.
        public int Count { get; set; }
        // Координаты аэропорта.
        public Coordinate airportCoord { get; set; }
        // Конструктор для создания нового объекта. 
        public Airport(Coordinate coordinate, int count) : base(coordinate)
        {
            airportCoord = coordinate;
            Count = count;
        }
        // Отправляет самолет в другой аэропорт и возвращает отправленный самолет.
        public Airplane FlyOff(int id, Airport destAirport)
        {
            var x = airportCoord.X + 100;
            var y = airportCoord.Y + 100;
            Airplane airpln = new Airplane(new Coordinate(x, y), 15000, false, id, destAirport);
            Count--;
            return airpln;
        }
    }
}