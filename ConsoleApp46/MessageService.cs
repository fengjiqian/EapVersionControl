using Ctg.Utility.Configuration;
using Ctg.Utility.RabbitMq;
using EAPClientManager;
using Fhec.Message.Eap;
using Fhec.Message.Emgt;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp46
{
    internal class MessageService
    {
        private ILog _logger = LogManager.GetLogger(typeof(MessageService));

        private SortedDictionary<string, EquipmentState> equipmentName_equipmentState_map;
        private List<GetConfigOutput> configOutputs;

        public InterProcessRabbitMq bus;

        public MessageService()
        {
            //this.bus = new InterProcessRabbitMq("host=172.16.204.243; virtualHost=forehope; username=forehope; password=forehope; persistentMessages=false");
            this.bus = new InterProcessRabbitMq("host=172.16.205.222; virtualHost=Tas; username=Tas; password=Tas; persistentMessages=false");
            //this.bus = new InterProcessRabbitMq("host=47.245.42.28; virtualHost=/; username=admin; password=123456; persistentMessages=false");
            //this.equipmentName_equipmentState_map = EAPClientManager._equipmentStateService.equipmentName_equipmentState_map;
            //this.configOutputs = EAPClientManager._configOutputs;

            RegisterMesssageHandler();
        }


        void RegisterMesssageHandler()
        {

            //bus.Respond<UpdateMacFileInput, UpdateMacFileOutput>(RedirectUpdateMacFileInput, "BPM.UpdateMacFileInput");
            //bus.Respond<GetConfigInput, GetConfigOutput>(GetEapConfig, "EAPClientManager.GetConfigInput");
            //bus.Subscribe<EquipmentState>("EAPClientManager.EquipmentState", HandleEquipmentState, "#");
            //bus.Subscribe<StdfStatisticsEvent>("EAPClientManager.StdfStatisticsEvent", HandleStdfStatisticsEvent, "#");
            //bus.Subscribe<MachineIpChangeEvent>("EAPClientManager.MachineIpChangeEvent", OnMachineIpChange, "#");


            bus.Subscribe<MachineStateChangeEvent>("EAPClientManager.StateChangeEvent", HandleStateChangeEvent, "#");

        }


        int i = 0;
        private void OnMachineIpChange(MachineIpChangeEvent obj)
        {
            Debug.WriteLine(i++);
            _logger.Debug("#HandleStdfStatisticsEvent");
            _logger.Debug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        private UpdateMacFileOutput RedirectUpdateMacFileInput(UpdateMacFileInput arg)
        {
            return new UpdateMacFileOutput();
        }

        private void HandleStdfStatisticsEvent(StdfStatisticsEvent obj)
        {
            _logger.Debug("#HandleStdfStatisticsEvent");
            _logger.Debug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        private void HandleStateChangeEvent(MachineStateChangeEvent obj)
        {
            _logger.Debug("#HandleStateChangeEvent");
            _logger.Debug(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        private void HandleEquipmentState(EquipmentState equipmentState)
        {


            _logger.Debug("#HandleEquipmentState");
            _logger.Debug(JsonConvert.SerializeObject(equipmentState, Formatting.Indented));

            //if (!_configOutputs.Exists(e => e.EquipmentName.Equals(equipmentState.EquipmentName))) return;


            if (equipmentName_equipmentState_map.ContainsKey(equipmentState.EquipmentName))
            {
                equipmentName_equipmentState_map.Remove(equipmentState.EquipmentName);
            }

            equipmentState.LastContactTime = DateTime.Now;
            //equipmentState.ModelName= _Equipment_EquipmentState_dic

            try
            {
                //equipmentState.ModelName = configOutputs.Find(e => e.EquipmentName.Equals(equipmentState.EquipmentName)).ModelName.Split(".")[4].Replace("FileMonitor", "").ToUpperInvariant();
                equipmentState.ModelName = configOutputs.Find(e => e.EquipmentName.Equals(equipmentState.EquipmentName)).ModelName.Split(".")[5].Replace("FileMonitor", "").ToUpperInvariant();


                equipmentName_equipmentState_map.Add(equipmentState.EquipmentName, equipmentState);

                File.WriteAllText(@"logs\_ls.json", JsonConvert.SerializeObject(equipmentName_equipmentState_map.Values, Formatting.Indented));
                File.WriteAllText(@"logs\_equipmentName_equipmentState_map.json", JsonConvert.SerializeObject(equipmentName_equipmentState_map, Formatting.Indented));
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }


            //File.WriteAllText(@"\\172.16.200.200\七剑项目部\FENGJIQIAN\eapclientmanager\_ls.json", JsonConvert.SerializeObject(equipmentName_equipmentState_map.Values, Formatting.Indented));
            //File.WriteAllText(@"\\172.16.200.200\七剑项目部\FENGJIQIAN\eapclientmanager\_equipmentName_equipmentState_map.json", JsonConvert.SerializeObject(equipmentName_equipmentState_map, Formatting.Indented));

        }

        public GetConfigOutput GetEapConfig(GetConfigInput getConfigInput)
        {
            _logger.Debug(JsonConvert.SerializeObject(getConfigInput, Formatting.Indented));
            Thread.Sleep(1000);
            return configOutputs.FindLast(e => e.EquipmentName.Equals(getConfigInput.EquipmentName, StringComparison.InvariantCultureIgnoreCase));
        }


    }
}
