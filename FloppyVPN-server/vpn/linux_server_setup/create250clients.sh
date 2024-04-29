#!/bin/bash

# This is a sample script that generates 250 (251) clients on a server.
# To be only used for tests or as an example of how wg (awg) config provisioning works

# Create required folders
mkdir -p /etc/amnezia/amneziawg
mkdir -p /root/Clients

# Generate server keys
awg genkey | tee /root/privateServer.key | awg pubkey > /root/publicServer.key
serverPrivateKey=$(cat /root/privateServer.key)
serverPublicKey=$(cat /root/publicServer.key)

# Get external server IP
serverIP=$(curl -s ipv4.icanhazip.com)

# Create server config awg0.conf
cat > /etc/amnezia/amneziawg/awg0.conf <<EOF
# ENDPOINT $serverIP
[Interface]
Address = 10.7.0.0/24
PrivateKey = $serverPrivateKey
ListenPort = 51235
PostUp = iptables -t nat -A POSTROUTING -s 10.7.0.0/24 -o eth0 -j SNAT --to-source $serverIP
PostDown = iptables -t nat -D POSTROUTING -s 10.7.0.0/24 -o eth0 -j SNAT --to-source $serverIP
Jc = 5
Jmin = 20
Jmax = 1180
S1 = 127
S2 = 59
H1 = 1012599525
H2 = 1289564734
H3 = 1183456273
H4 = 2112541503
EOF

# Create configs from 1 to 250 and additional test.conf от 1 до 250 и дополнительный test.conf
for i in {1..251}; do
  awg genkey | tee /root/privateClient$i.key | awg pubkey > /root/publicClient$i.key
  clientPrivateKey=$(cat /root/privateClient$i.key)
  clientPublicKey=$(cat /root/publicClient$i.key)

  clientPSK=$(awg genpsk)

  if [ $i -eq 251 ]; then
    # For test.conf
    clientFileName="client_test"
    clientName="client_test"
  else
    # For each client
    clientFileName="client$i"
    clientName="client$i"
  fi

  cat >> /etc/amnezia/amneziawg/awg0.conf <<EOF

#$clientName
[Peer]
PublicKey = $clientPublicKey
PresharedKey = $clientPSK
AllowedIPs = 10.7.0.$i/32
EOF

  cat > /root/Clients/$clientFileName.conf <<EOF
[Interface]
Address = 10.7.0.$i/32
DNS = 1.1.1.1, 1.0.0.1
PrivateKey = $clientPrivateKey
Jc = 5
Jmin = 20
Jmax = 1180
S1 = 127
S2 = 59
H1 = 1012599525
H2 = 1289564734
H3 = 1183456273
H4 = 2112541503

[Peer]
Endpoint = $serverIP:51235
PublicKey = $serverPublicKey
PresharedKey = $clientPSK
AllowedIPs = 0.0.0.0/0
PersistentKeepalive = 25
EOF

  # Delete temp key files
  rm /root/privateClient$i.key /root/publicClient$i.key
done

# Delete server key files
rm /root/publicServer.key
rm /root/privateServer.key

# Print success message
echo -e "\033[0;32mDONE: awg0, 250 clients configs and 1 test config!\033[0m"
