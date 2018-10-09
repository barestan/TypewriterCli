using System;

namespace Typewriter.Demo
{
    public class TestSubModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

    }

    public class TestModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TestSubModel SubModel { get; set; } 
    }

}