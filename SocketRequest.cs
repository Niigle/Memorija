using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Memorija
{
    public class SocketRequest
    {

        public string TipPoruke { get; set; }

        //[System.Text.Json.Serialization.JsonIgnore]
        public string Json { get; set; }

        public SocketRequest() { }

        public SocketRequest(string tipPoruke)
        {
            this.TipPoruke = tipPoruke;
        }

        public void Pack()
        {
            string payload = JsonSerializer.Serialize(this, this.GetType());

            var envelope = new { TipPoruke = this.TipPoruke, Json = payload };
            this.Json = JsonSerializer.Serialize(envelope);
        }
        /*
        public void Pack()
        {
            var fields = this.GetType()
                .GetProperties()
                .Where(p => p.Name != nameof(Json))
                .ToDictionary(p => p.Name, p => p.GetValue(this));

            string payload = JsonSerializer.Serialize(fields);
            this.Json = JsonSerializer.Serialize(new { TipPoruke = this.TipPoruke, Json = payload });
        }*/
    }
}
