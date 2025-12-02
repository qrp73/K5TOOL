# CMakeLists.txt for C# v 1.00
# (c)2025 qrp73 https://github.com/qrp73/K5TOOL
#-----------------------------------------------------------------------
cmake_minimum_required(VERSION 3.10)
project(k5tool)

# Sources
set(SRC 
    Program.cs
    Utils.cs
    Device.cs
    Logger.cs
    CommandHelper.cs
    FirmwareHelper.cs
    CommandLineParser.cs
    Packets/FirmwareConstraints.cs
    Packets/Envelope.cs
    Packets/Packet.cs
    Packets/PacketHelloReq.cs
    Packets/PacketHelloAck.cs
    Packets/PacketHelloTestReq.cs
    Packets/PacketReadEepromReq.cs
    Packets/PacketReadEepromAck.cs
    Packets/PacketFlashBeaconAck.cs
    Packets/PacketWriteEepromReq.cs
    Packets/PacketWriteEepromAck.cs
    Packets/PacketReadAdcReq.cs
    Packets/PacketReadAdcAck.cs
    Packets/PacketRebootReq.cs
    Packets/PacketFlashVersionReq.cs
    Packets/PacketFlashWriteReq.cs
    Packets/PacketFlashWriteAck.cs
    Packets/PacketReadRssiReq.cs
    Packets/PacketReadRssiAck.cs
    Packets/ProtocolBase.cs
    Packets/V2/ProtocolV2.cs
    Packets/V2/Packet2FlashBeaconAck.cs
    Packets/V2/Packet2FlashVersionReq.cs
    Packets/V2/Packet2FlashWriteReq.cs
    Packets/V2/Packet2FlashWriteAck.cs
    Packets/V5/ProtocolV5.cs
    Packets/V5/Packet5FlashBeaconAck.cs
    Packets/V5/Packet5FlashVersionReq.cs
    Packets/V5/Packet5FlashWriteReq.cs
    Packets/V5/Packet5FlashWriteAck.cs
    AssemblyInfo.cs
)

# References
set(REFS
)

# Compiler flags
set(COMPILE_OPTIONS
)

#-----------------------------------------------------------------------
# Define colors
if(NOT WIN32)
  string(ASCII 27 Esc)
  set(ColorCompile  "${Esc}[32m")
  set(ColorReset    "${Esc}[0m")
endif()

if(NOT WIN32)
    # Check mono-runtime
    message(STATUS "Check mono-runtime")
    find_program(MONO_EXECUTABLE mono)
    if(NOT MONO_EXECUTABLE)
        message(FATAL_ERROR "mono-runtime not found! Please install with `apt install mono-runtime`")
    endif()
endif()

# Check C# compiler
message(STATUS "Detecting C# compiler")
if(NOT CMAKE_CS_COMPILER)
    find_program(CMAKE_CS_COMPILER mcs)
endif()
if(NOT CMAKE_CS_COMPILER)
    find_program(CMAKE_CS_COMPILER csc)
    if(CMAKE_CS_COMPILER)
        list(APPEND COMPILE_OPTIONS -nologo)
    endif()
endif()
if(NOT CMAKE_CS_COMPILER)
    message(FATAL_ERROR "C# compiler not found! Please install with `apt install mono-mcs`")
else()
    message(STATUS "Detected C# compiler: ${CMAKE_CS_COMPILER}")
endif()

# Create temporary empty file for reference libraries check
set(TMP_CS  "${CMAKE_BINARY_DIR}/tmp_check.cs")
set(TMP_OUT "${CMAKE_BINARY_DIR}/tmp_check.dll")
file(WRITE ${TMP_CS} "// empty file for library check\n")
# do test compile for reference libraries check
foreach(lib ${REFS})
    message(STATUS "Check library ${lib}")
    execute_process(
        COMMAND ${CMAKE_CS_COMPILER} -nologo -target:library -out:${TMP_OUT} -r:${lib} ${TMP_CS}
        RESULT_VARIABLE res
        OUTPUT_QUIET
        ERROR_QUIET
    )
    if(NOT res EQUAL 0)
        file(REMOVE ${TMP_CS})
        file(REMOVE ${TMP_OUT})
        message(FATAL_ERROR "Required C# library '${lib}' not found! Please install Mono/SDK.")
    endif()
endforeach()
# Remove temporary file
file(REMOVE ${TMP_CS})
file(REMOVE ${TMP_OUT})

# Build MCS options
set(MCS_OPTS "")
if (CMAKE_BUILD_TYPE STREQUAL "Debug")
    list(APPEND MCS_OPTS 
        -debug
        -define:DEBUG
    )
endif()
list(APPEND MCS_OPTS ${COMPILE_OPTIONS})
list(APPEND MCS_OPTS "-out:${CMAKE_BINARY_DIR}/${PROJECT_NAME}.exe")
foreach(r ${REFS})
    list(APPEND MCS_OPTS "-r:${r}")
endforeach()
foreach(src ${SRC})
    if (IS_ABSOLUTE "${src}")
        list(APPEND MCS_OPTS "${src}")
    else()
        list(APPEND MCS_OPTS "${CMAKE_SOURCE_DIR}/${src}")
    endif()
endforeach()

# Compile
#message(STATUS "${CMAKE_CS_COMPILER} ${MCS_OPTS}")
add_custom_target(${PROJECT_NAME} ALL
    COMMAND ${CMAKE_CS_COMPILER} ${MCS_OPTS}
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
    COMMENT "${ColorCompile}Compiling C# executable ${PROJECT_NAME}${ColorReset}"
)

# Create launcher script using configure_file
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    set(LAUNCHER_TEMPLATE ${CMAKE_SOURCE_DIR}/launcher.debug.in)
else()
    set(LAUNCHER_TEMPLATE ${CMAKE_SOURCE_DIR}/launcher.release.in)
endif()
set(LAUNCHER_EXE "\$(dirname \${BASH_SOURCE[0]:-\$0})/${PROJECT_NAME}.exe")
set(LAUNCHER_OUTPUT_LOCAL ${CMAKE_BINARY_DIR}/${PROJECT_NAME})
configure_file(${LAUNCHER_TEMPLATE} ${LAUNCHER_OUTPUT_LOCAL} @ONLY)
if(NOT WIN32)
    add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
        COMMAND chmod +x ${LAUNCHER_OUTPUT_LOCAL}
        COMMENT "Create local launcher script ${PROJECT_NAME}"
    )
endif()


# Install exe to ${CMAKE_INSTALL_PREFIX}/lib/
install(FILES ${CMAKE_BINARY_DIR}/${PROJECT_NAME}.exe
        DESTINATION lib/${PROJECT_NAME})

# Install launcher to ${CMAKE_INSTALL_PREFIX}/bin/
set(LAUNCHER_EXE "${CMAKE_INSTALL_PREFIX}/lib/${PROJECT_NAME}/${PROJECT_NAME}.exe")
set(LAUNCHER_OUTPUT_INSTALL ${CMAKE_BINARY_DIR}/${PROJECT_NAME}_install)
configure_file(${LAUNCHER_TEMPLATE} ${LAUNCHER_OUTPUT_INSTALL} @ONLY)
install(FILES ${LAUNCHER_OUTPUT_INSTALL}
        DESTINATION bin
        RENAME ${PROJECT_NAME}
        PERMISSIONS OWNER_EXECUTE OWNER_WRITE OWNER_READ
                    GROUP_EXECUTE GROUP_READ
                    WORLD_EXECUTE WORLD_READ)
