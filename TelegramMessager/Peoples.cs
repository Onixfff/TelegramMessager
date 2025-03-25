using System.Collections.Generic;

namespace TelegramMessager
{
    internal class Peoples
    {
        private List<People> _peoples;

        public Peoples()
        {
            _peoples = new List<People>()
                    {
                        new People(787471566),
                        new People(961317657), //Владимир Викторович
                        new People(1973965023), //Татьяна Владимировна
                        new People(805032669) // Артем Данишевский
                    };
        }

        public List<People> GetListPeoples()
        {
            return _peoples;
        }
    }
}
