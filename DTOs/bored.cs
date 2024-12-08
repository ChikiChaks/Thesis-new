using System;
namespace Prog3_WebApi_Javascript.DTOs
{
    //ניצור DTO נוסף עבור הprompt שמגיע מהמשתמש
    //כרגע הוא יכיל רק מאפיין של תוכן, בהמשך נוכל להוסיף עוד מאפיינים

    public class bored
    {
        public string activity { get; set; }
        public string type { get; set; }
        public string participants { get; set; }
    }

}

