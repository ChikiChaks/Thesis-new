using System;
namespace Prog3_WebApi_Javascript.DTOs
{
    //כדי לשלוח מידע אל הAPI נצטרך ליצור אותו בתצורה בה הAPI מצפה לקבל אותו. נשלח לAPI את המידע הבא:

    public class GPTRequest
    {
        public string model { get; set; } // השם של המודל צ׳אט גיפיטי 4/3 ועוד
        public List<Message> messages { get; set; } = new List<Message>(); //זו רשימה, כי יכולה להיות יותר מהודעה אחת
        //public int max_tokens { get; set; } //מקסימום אורך של הודעה  
        public double temperature { get; set; }
        public object response_format { get; set; } //החזרת תשובה כJSON 


    }

}

