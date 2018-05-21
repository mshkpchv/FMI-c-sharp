﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using aspdota.Exceptions;
using aspdota.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace aspdota.Serializer
{
    public class Reader<T> : IReader<T>
    {
        private static string FILESYSTEM = "/Users/mihailkopchev/Projects/asp-dota/asp-dota/XML";
        private static string EXTENSION = ".xml";
        private string currentFile = "";
        private XmlReaderSettings _settings;
        private XmlSerializer _xmlSerializer; 
        private ILogger _logger;

        public Reader (){}
        public Reader(ILogger logger){
            _logger = logger;
        }

        public T Deserialize(TextReader reader)
        {
            try
            {
                this._xmlSerializer = new XmlSerializer(typeof(T));
                T obj = (T)this._xmlSerializer.Deserialize(reader);
                return obj;
            }
            catch (Exception e)
            {
                throw new CannotDeserializeException(e.Message, e.GetBaseException());
            }
        }

        public void Serialize(T obj, TextWriter streamWriter)
        {
            try
            {
                this._xmlSerializer = new XmlSerializer(typeof(T));
                this._xmlSerializer.Serialize(streamWriter, obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public T Deserialize(string file){
            return this.Deserialize(new StreamReader(file));

        }

        public void Serialize(T obj,string where){
             this.Serialize(obj, new StreamWriter(where));

        }
        public void ValidateContent(string filesystem)
        {
            string fs = filesystem != null ? filesystem : FILESYSTEM;
            InitSettings(ValidationType.DTD,DtdProcessing.Parse);
            InitResolver(fs);
            useEventHandler();

            try
            {
                string[] files = GetFiles(fs);

                foreach(string file in files){
                    if(IsCorrectFile(file)){
                        currentFile = file;
                        XmlReader reader = XmlReader.Create(file, _settings);
                        while(reader.Read()){
                            // if exception occur ValidationCallback is called to interrupt the process
                        }
                        Console.WriteLine("------> file : " + currentFile + " is valid");

                    }

                }

            }
            catch (Exception e)
            {
               Console.WriteLine("Problem with file: " + currentFile + " ,cause +" + e.Message);

            }
        }


        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
                Console.WriteLine("Problem with file: " + currentFile + ", cause + " + e.Message);
        }

        private string[] GetFiles(string fs)
        {
            return Directory.Exists(fs) ? Directory.GetFiles(fs) : null;

        }
        private static bool IsCorrectFile(string file)
        {
            return file.EndsWith(EXTENSION);
        }
        private void InitSettings(ValidationType type, DtdProcessing dtdProcessing)
        {

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = type,
                DtdProcessing = dtdProcessing,
                IgnoreWhitespace = true
            };
            _settings = settings;
        }
        private void InitResolver(string fs)
        {
            XmlUrlResolver xmlResolver = new XmlUrlResolver();
            xmlResolver.ResolveUri(null, fs);
            _settings.XmlResolver = xmlResolver;
        }
        private void useEventHandler()
        {
            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationCallBack);
            _settings.ValidationEventHandler += eventHandler;
        }


    }
}