﻿namespace Scripts.Event
{

    public delegate void EventListener<in TEvent>(object sender, TEvent @event);

}