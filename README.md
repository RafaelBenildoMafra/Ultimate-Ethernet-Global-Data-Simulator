Important to notice that all the work within this repository has been solely accomplished by myself, independent of any commercial intentions for any company. 
Throughout the development process, I've relied on free tools such as Wireshark, Packet Sender, and the demo version of KEPServerEX. 

https://www.wireshark.org

https://packetsender.com

https://www.ptc.com/en/products/kepware/kepserverex

The EGD protocol was developed in collaboration between GE Fanuc Automation and GE Drive Systems in 1998, 
with the aim of enabling data exchange between PLCs and computers. It was designed to provide flexibility and coordination between control devices, 
enabling the efficient transmission of critical information in industrial environments.

The protocol is based on UDP to share information between controllers, 
exchanging data samples, as illustrated in the figure below. Each packet contains a data sample or snapshot of a controller's memory. 
These samples are periodically sent to one or more peer controllers that store the data for use in application tasks; 
controllers can both read from and write to the network. Each data sample is uniquely identified to relate it to a definition describing the data it contains, 
called an Exchange. The controller that generates the sample is called the Producer, and the controller that receives it is called the Consumer. 
Each controller will send or receive data samples only for the Exchanges it has been configured for.
In this way, a network can be configured for multiple controllers to share information to perform control or monitoring functions.

<img width="400" alt="egd_plc" src="https://github.com/RafaelBenildoMafra/Ultimate-Ethernet-Global-Data-Simulator/assets/62677441/c4b14713-7f77-4228-8d52-4eb747ba291b">

To ensure the reliability of the transmitted information at the application layer, EGD organizes the data to be transmitted into Exchanges, 
and several Exchanges are combined to form Pages. Each Page has a unique identifier, which is a combination of the Producer ID and the Exchange ID. 
This identifier allows the receiver to recognize the data and know where to store it. With EGD, a Producer can send information to an unlimited number of Consumers 
simultaneously, at a fixed periodic rate.

Each Exchange within EGD contains a Configuration Signature, indicating the configuration revision number of the Exchange. 
When the Consumer receives a message from the Producer, it checks if the Major signature numbers match and if the Minor signature number of the Consumer is less than 
or equal to the Minor signature number of the Producer. If these conditions are met, the Consumer accepts the received message because it contains more data than expected.

Its structured organization in Producer ID and Exchange ID, along with the verification of Configuration Signatures, ensures the integrity of the transmitted data and synchronization 
between Producers and Consumers. Figure below displays all the fields of the EGD protocol.

<img width="350" alt="egd3" src="https://github.com/RafaelBenildoMafra/Ultimate-Ethernet-Global-Data-Simulator/assets/62677441/73f1f670-d96d-4de1-9ea6-cb64ac1c841a">

