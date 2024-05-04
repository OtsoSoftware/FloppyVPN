##### Updating linux kernel
##### amneziawg installing
##### installing amneziawg-tools
##### setting AmneziaWG: awg0 + client config
_______________________________

#0
apt update && apt dist-upgrade -y
reboot
apt install linux-headers-$(uname -r)

#1
apt autoremove -y
apt-get update && apt-get full-upgrade -y

#2
apt install -y unzip
apt install -y iptables
apt install -y nftables
apt install -y git

#3 make sure at least one repo ("deb-src...") exists and is not commented
vi /etc/apt/sources.list

#4
apt install -y software-properties-common python3-launchpadlib gnupg2 linux-headers-$(uname -r)
apt install -y dkms



##### Installing amneziawg

# Put "amneziawg-X.tar" in the /root (X stands for the patch that suits the kernel version)
# then:
tar xf amneziawg-X.tar
mv amneziawg-X /usr/src
dkms install amneziawg/X
modprobe amneziawg
lsmod | grep amneziawg




##### Installing amneziawg-tools
cd /root/
git clone https://github.com/amnezia-vpn/amneziawg-tools
cd /root/amneziawg-tools/src
make /root/amneziawg-tools/src
make install

# check if we successfully installed amneziawg-tools
awg-quick -v



##### Setting AmneziaWG

#0 Delete default awg0
ip li add dev awg0 type amneziawg

# Setup forwarding
sysctl -w net.ipv4.conf.all.forwarding=1

### if ipv6 supported:
sysctl -w net.ipv6.conf.all.forwarding=1

sysctl -p


*now please put compiled vpn server build on the server!*


### assume /root/FloppyVPN_vpn_server/FloppyVPN-server-vpn is a full path to executable

chmod +x /root/FloppyVPN_vpn_server/FloppyVPN-server-vpn

*now launch server, close server and fill CONFIG.XML file!*

touch /etc/systemd/system/floppyvpn_server.service
chmod 664 /etc/systemd/system/floppyvpn_server.service



echo "[Unit]
Description=FloppyVPN vpn server

[Service]
ExecStart=/root/FloppyVPN_vpn_server/FloppyVPN-server-vpn

[Install]
WantedBy=multi-user.target" > /etc/systemd/system/floppyvpn_server.service



systemctl daemon-reload
systemctl enable floppyvpn_server

systemctl start floppyvpn_server
systemctl status floppyvpn_server

### now the service is running in the background!
### make sure to enter the new vpn server into system using, say, admin panel. gl hf!







# Continue reading this document only to learn how stuff is done manually


# Use script to create sample server and client configs
*use script*

# Launch awg0
awg-quick down awg0
awg-quick up awg0
awg

# How to refresh awg0 without clients sessions interruptions:
awg syncconf awg0 <(awg-quick strip awg0)
